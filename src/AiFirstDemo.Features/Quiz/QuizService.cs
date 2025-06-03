using AiFirstDemo.Features.Quiz.Models;
using AiFirstDemo.Infrastructure.Redis;
using AiFirstDemo.Infrastructure.OpenAI;
using AiFirstDemo.Features.UserSessions;
using AiFirstDemo.Features.Shared.Models;
using AiFirstDemo.Features.Analytics;
using Microsoft.Extensions.Logging;

namespace AiFirstDemo.Features.Quiz;

public class QuizService : IQuizService
{
    private readonly IRedisService _redis;
    private readonly IOpenAIService _openAI;
    private readonly IUserSessionService _userSession;
    private readonly IAnalyticsService _analytics;
    private readonly ILogger<QuizService> _logger;
    private const string QUIZ_ATTEMPT_PREFIX = "quiz:attempt:";
    private const string QUIZ_QUESTIONS_KEY = "quiz:questions:all";

    public QuizService(
        IRedisService redis, 
        IOpenAIService openAI,
        IUserSessionService userSession,
        IAnalyticsService analytics,
        ILogger<QuizService> logger)
    {
        _redis = redis;
        _openAI = openAI;
        _userSession = userSession;
        _analytics = analytics;
        _logger = logger;
    }

    public async Task<List<QuizQuestion>> GetQuizQuestionsAsync()
    {
        // Try to get cached questions first
        var cachedQuestions = await _redis.GetAsync<List<QuizQuestion>>(QUIZ_QUESTIONS_KEY);
        if (cachedQuestions != null && cachedQuestions.Count > 0)
        {
            return cachedQuestions.Take(10).ToList();
        }

        // Generate new questions if cache is empty
        return await GenerateRandomQuestionsAsync(10);
    }

    public async Task<List<QuizQuestion>> GenerateRandomQuestionsAsync(int count = 10)
    {
        // For demo purposes, return predefined questions
        // In a real scenario, you might generate these dynamically with AI
        var questions = GetPredefinedQuestions();
        
        // Cache the questions
        await _redis.SetAsync(QUIZ_QUESTIONS_KEY, questions, TimeSpan.FromHours(1));
        
        return questions.Take(count).ToList();
    }

    public async Task<QuizResultResponse> SubmitQuizAsync(QuizSubmissionRequest request)
    {
        var session = await _userSession.GetSessionAsync(request.SessionId);
        if (session == null)
        {
            throw new InvalidOperationException("Invalid session");
        }

        // Check if this IP can still take quizzes
        // We need to get the original IP from the session's IP hash
        // For now, we'll use a workaround since we need the original IP
        // In a real implementation, we'd store the original IP or use the hash consistently
        var sessionIpHash = session.IpHash;
        
        // Create a temporary method to check attempts using IP hash
        var attempts = await GetQuizAttemptsForIpHashAsync(sessionIpHash);
        if (attempts >= 3)
        {
            throw new InvalidOperationException($"Quiz attempt limit reached. You have completed {attempts}/3 attempts today. Please try again tomorrow.");
        }

        // Check if this specific session already completed a quiz
        if (session.HasCompletedQuiz)
        {
            throw new InvalidOperationException("Quiz already completed for this session. Create a new session to try again.");
        }

        var questions = await GetQuizQuestionsAsync();
        var startTime = DateTime.UtcNow.AddMinutes(-15); // Assume 15 min quiz time
        var results = new List<QuestionResult>();
        var quizAnswers = new List<QuizAnswer>();
        int score = 0;

        foreach (var submission in request.Answers)
        {
            var question = questions.FirstOrDefault(q => q.Id == submission.QuestionId);
            if (question == null) continue;

            var isCorrect = question.CorrectAnswer.Equals(submission.SelectedAnswer, StringComparison.OrdinalIgnoreCase);
            if (isCorrect) score++;

            var quizAnswer = new QuizAnswer(
                QuestionId: submission.QuestionId,
                SelectedAnswer: submission.SelectedAnswer,
                IsCorrect: isCorrect,
                AnsweredAt: DateTime.UtcNow
            );
            quizAnswers.Add(quizAnswer);

            // Remove AI explanation to improve performance - user requested this optimization
            results.Add(new QuestionResult(
                QuestionText: question.Text,
                SelectedAnswer: submission.SelectedAnswer,
                CorrectAnswer: question.CorrectAnswer,
                IsCorrect: isCorrect,
                Explanation: "" // Removed AI explanation for performance
            ));
        }

        // Generate AI analysis
        var analysisData = quizAnswers.Select(a => {
            var q = questions.First(qu => qu.Id == a.QuestionId);
            return new QuizAnalysisData(
                Question: q.Text,
                UserAnswer: a.SelectedAnswer,
                CorrectAnswer: q.CorrectAnswer,
                IsCorrect: a.IsCorrect,
                Category: q.Category
            );
        }).ToList();

        var aiAnalysis = await _openAI.AnalyzeQuizPerformanceAsync(analysisData);
        var recommendations = await _openAI.GenerateTipsAsync("Cursor and AI Development", 3);

        // Create quiz attempt
        var attempt = new QuizAttempt(
            SessionId: request.SessionId,
            Answers: quizAnswers,
            Score: score,
            CompletedAt: DateTime.UtcNow,
            Duration: DateTime.UtcNow - startTime,
            AiAnalysis: aiAnalysis
        );

        // Save attempt
        await _redis.SetAsync($"{QUIZ_ATTEMPT_PREFIX}{request.SessionId}", attempt, TimeSpan.FromDays(7));
        
        // Mark quiz as completed (this will increment the attempt count)
        await _userSession.MarkQuizCompletedAsync(request.SessionId);

        // Track analytics
        await _analytics.TrackQuizCompletionAsync(request.SessionId, score);

        _logger.LogInformation("Quiz completed for session {SessionId} with score {Score}/{Total}", 
            request.SessionId, score, questions.Count);

        return new QuizResultResponse(
            Score: score,
            TotalQuestions: questions.Count,
            Percentage: Math.Round((double)score / questions.Count * 100, 1),
            Results: results,
            AiAnalysis: aiAnalysis,
            Recommendations: recommendations
        );
    }

    public async Task<QuizAttempt?> GetQuizAttemptAsync(string sessionId)
    {
        return await _redis.GetAsync<QuizAttempt>($"{QUIZ_ATTEMPT_PREFIX}{sessionId}");
    }

    private List<QuizQuestion> GetPredefinedQuestions()
    {
        return new List<QuizQuestion>
        {
            new("1", "What is the primary keyboard shortcut to open Cursor's AI chat?", 
                new List<string> { "Ctrl+K", "Ctrl+Shift+P", "Ctrl+L", "Ctrl+Alt+C" }, 
                "Ctrl+L", "Cursor Basics", "Easy"),
            
            new("2", "Which AI model is best for code generation tasks in Cursor?", 
                new List<string> { "GPT-3.5", "GPT-4", "Claude-3", "It depends on the task" }, 
                "It depends on the task", "AI Integration", "Medium"),
            
            new("3", "What does the '@' symbol do in Cursor's AI chat?", 
                new List<string> { "Mentions a user", "References files/symbols", "Creates a comment", "Starts a new chat" }, 
                "References files/symbols", "Context Management", "Easy"),
            
            new("4", "When should you use Cursor's Composer vs Chat feature?", 
                new List<string> { "Always use Chat", "Composer for multi-file edits, Chat for questions", "Always use Composer", "They're the same thing" }, 
                "Composer for multi-file edits, Chat for questions", "Best Practices", "Medium"),
            
            new("5", "What is the best practice for writing prompts in Cursor?", 
                new List<string> { "Be vague to let AI decide", "Write very long detailed prompts", "Be specific but concise", "Use only technical jargon" }, 
                "Be specific but concise", "Prompt Engineering", "Medium"),
            
            new("6", "How can you provide better context to Cursor's AI?", 
                new List<string> { "Use @files and @symbols", "Write longer prompts", "Use more technical terms", "Ask multiple questions at once" }, 
                "Use @files and @symbols", "Context Management", "Easy"),
            
            new("7", "What does Ctrl+K do in Cursor?", 
                new List<string> { "Opens file search", "Inline code generation", "Opens settings", "Saves the file" }, 
                "Inline code generation", "Cursor Basics", "Easy"),
            
            new("8", "When working with large codebases, what's the most effective way to use Cursor?", 
                new List<string> { "Reference specific files with @", "Ask general questions", "Use only global context", "Avoid using context" }, 
                "Reference specific files with @", "Advanced Features", "Medium"),
            
            new("9", "What's the difference between Cursor's Apply and Edit modes?", 
                new List<string> { "No difference", "Apply accepts changes, Edit lets you review", "Edit is for new code, Apply for existing", "Apply is automatic, Edit is manual" }, 
                "Apply accepts changes, Edit lets you review", "Advanced Features", "Hard"),
            
            new("10", "Which practice improves AI code generation quality the most?", 
                new List<string> { "Using longer variable names", "Providing clear context and examples", "Writing comments in ALL CAPS", "Using only single-letter variables" }, 
                "Providing clear context and examples", "Best Practices", "Medium")
        };
    }

    private string GetIpFromSession(AiFirstDemo.Features.Shared.Models.UserSession session)
    {
        // For now, we'll use a placeholder since we don't store the original IP
        // In a real implementation, you might store the original IP or derive it differently
        return "placeholder-ip"; // This should be replaced with actual IP retrieval logic
    }

    private async Task<int> GetQuizAttemptsForIpHashAsync(string ipHash)
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var key = $"ip:quiz:attempts:{ipHash}:{today}";
        
        try
        {
            var attemptsStr = await _redis.GetStringAsync(key);
            return int.TryParse(attemptsStr, out var attempts) ? attempts : 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting quiz attempts for IP hash {IpHash}, returning 0", ipHash);
            return 0;
        }
    }

    public async Task<string?> GetQuestionHintAsync(string questionId)
    {
        try
        {
            _logger.LogInformation("Getting hint for question {QuestionId}", questionId);
            
            // Get the predefined questions to find the specific question
            var questions = GetPredefinedQuestions();
            var question = questions.FirstOrDefault(q => q.Id == questionId);
            
            if (question == null)
            {
                _logger.LogWarning("Question with ID {QuestionId} not found", questionId);
                return null;
            }

            // Don't provide hints for hard questions to maintain scoring integrity
            if (question.Difficulty.Equals("Hard", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Hint requested for hard question {QuestionId}, but hints are not available for hard questions", questionId);
                return null;
            }

            // Check cache first
            var cacheKey = $"quiz:hint:{questionId}";
            var cachedHint = await _redis.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedHint))
            {
                _logger.LogInformation("Returning cached hint for question {QuestionId}", questionId);
                return cachedHint;
            }

            _logger.LogInformation("Generating new hint for question {QuestionId} using OpenAI", questionId);

            // Generate hint using OpenAI with better error handling
            var hint = await _openAI.GenerateQuizHintAsync(question.Text, question.Options, question.Category);
            
            if (string.IsNullOrEmpty(hint))
            {
                _logger.LogWarning("OpenAI returned empty hint for question {QuestionId}", questionId);
                hint = GetFallbackHint(question);
            }
            
            // Cache the hint for 24 hours
            await _redis.SetAsync(cacheKey, hint, TimeSpan.FromHours(24));
            
            _logger.LogInformation("Successfully generated and cached hint for question {QuestionId}", questionId);
            return hint;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating hint for question {QuestionId}", questionId);
            
            // Return a helpful fallback hint based on the question category
            var questions = GetPredefinedQuestions();
            var question = questions.FirstOrDefault(q => q.Id == questionId);
            if (question != null)
            {
                return GetFallbackHint(question);
            }
            
            return "Think about the key concepts and best practices related to this question. Consider what would be most practical and commonly used in real development scenarios.";
        }
    }

    private string GetFallbackHint(QuizQuestion question)
    {
        return question.Category.ToLower() switch
        {
            "cursor basics" => "Think about the most commonly used keyboard shortcuts and features that improve productivity in Cursor.",
            "ai integration" => "Consider which AI model characteristics matter most for different types of development tasks.",
            "context management" => "Focus on how you can provide better context to AI tools for more accurate assistance.",
            "best practices" => "Think about what would be most practical and efficient in real development workflows.",
            "prompt engineering" => "Consider what makes prompts clear, specific, and actionable for AI assistants.",
            "advanced features" => "Think about the difference between review-based and automatic features in AI development tools.",
            _ => "Consider the core principles and most practical approaches related to this topic."
        };
    }
}