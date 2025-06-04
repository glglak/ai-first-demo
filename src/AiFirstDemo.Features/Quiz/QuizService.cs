using AiFirstDemo.Features.Quiz.Models;
using AiFirstDemo.Infrastructure.Redis;
using AiFirstDemo.Infrastructure.OpenAI;
using AiFirstDemo.Features.UserSessions;
using AiFirstDemo.Features.Shared.Models;
using Microsoft.Extensions.Logging;

namespace AiFirstDemo.Features.Quiz;

public class QuizService : IQuizService
{
    private readonly IRedisService _redis;
    private readonly IOpenAIService _openAI;
    private readonly IUserSessionService _userSession;
    private readonly ILogger<QuizService> _logger;
    private const string QUIZ_ATTEMPT_PREFIX = "quiz:attempt:";
    private const string QUIZ_QUESTIONS_KEY = "quiz:questions:all";

    public QuizService(
        IRedisService redis, 
        IOpenAIService openAI,
        IUserSessionService userSession,
        ILogger<QuizService> logger)
    {
        _redis = redis;
        _openAI = openAI;
        _userSession = userSession;
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
        var startTime = DateTime.UtcNow;
        
        // Get session to get IP information
        var session = await _userSession.GetSessionAsync(request.SessionId);
        if (session == null)
        {
            throw new InvalidOperationException("Invalid session");
        }

        // Track quiz attempt number for this IP
        var ipHash = session.IpHash;
        var attemptNumber = await IncrementQuizAttemptForIpAsync(ipHash);
        
        // Get questions
        var questions = await GetQuizQuestionsAsync();
        if (questions.Count == 0)
        {
            throw new InvalidOperationException("No quiz questions available");
        }

        // Validate answers
        if (request.Answers.Count != questions.Count)
        {
            throw new InvalidOperationException("Answer count doesn't match question count");
        }

        // Calculate score
        int score = 0;
        var results = new List<QuestionResult>();
        var quizAnswers = new List<QuizAnswer>();

        foreach (var submission in request.Answers)
        {
            var question = questions.FirstOrDefault(q => q.Id == submission.QuestionId);
            if (question == null) continue;
            
            var isCorrect = string.Equals(question.CorrectAnswer, submission.SelectedAnswer, StringComparison.OrdinalIgnoreCase);
            if (isCorrect) score++;

            results.Add(new QuestionResult(
                QuestionText: question.Text,
                SelectedAnswer: submission.SelectedAnswer,
                CorrectAnswer: question.CorrectAnswer,
                IsCorrect: isCorrect,
                Explanation: "" // Removed AI explanation for performance
            ));

            quizAnswers.Add(new QuizAnswer(
                QuestionId: question.Id,
                SelectedAnswer: submission.SelectedAnswer,
                IsCorrect: isCorrect,
                AnsweredAt: DateTime.UtcNow
            ));
        }

        // This was causing slow quiz submissions, so we'll provide static feedback instead
        var aiAnalysis = GetStaticAnalysisFeedback(score, questions.Count);
        var recommendations = GetStaticRecommendations();

        // Create quiz attempt with IP attempt tracking
        var attempt = new QuizAttempt(
            SessionId: request.SessionId,
            Answers: quizAnswers,
            Score: score,
            CompletedAt: DateTime.UtcNow,
            Duration: DateTime.UtcNow - startTime,
            AiAnalysis: aiAnalysis
        );

        // Save attempt with extended key for IP tracking
        var attemptKey = $"{QUIZ_ATTEMPT_PREFIX}{request.SessionId}";
        await _redis.SetAsync(attemptKey, attempt, TimeSpan.FromDays(30)); // Changed to 30 days to match game scores
        
        // Also store with IP attempt number for analytics
        var ipAttemptKey = $"quiz:ip:attempt:{ipHash}:{attemptNumber}:{request.SessionId}";
        await _redis.SetAsync(ipAttemptKey, attempt, TimeSpan.FromDays(30));
        
        // Mark quiz as completed (this will increment the attempt count)
        await _userSession.MarkQuizCompletedAsync(request.SessionId);

        // Track analytics directly
        await TrackAnalyticsAsync(request.SessionId, score);

        _logger.LogInformation("Quiz completed for session {SessionId} with score {Score}/{Total}, IP attempt #{AttemptNumber}", 
            request.SessionId, score, questions.Count, attemptNumber);

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

    private string GetStaticAnalysisFeedback(int score, int totalQuestions)
    {
        var percentage = Math.Round((double)score / totalQuestions * 100, 1);
        
        return percentage switch
        {
            >= 90 => $"🎉 Excellent work! You scored {score}/{totalQuestions} ({percentage}%). You have a strong understanding of AI-First development practices.",
            >= 80 => $"👍 Great job! You scored {score}/{totalQuestions} ({percentage}%). You're well on your way to mastering AI-assisted development.",
            >= 70 => $"✅ Good effort! You scored {score}/{totalQuestions} ({percentage}%). Keep practicing and exploring AI development tools.",
            >= 60 => $"📚 Not bad! You scored {score}/{totalQuestions} ({percentage}%). Consider reviewing the tips section for more insights.",
            _ => $"💪 Keep learning! You scored {score}/{totalQuestions} ({percentage}%). This is a great start - check out our tips and try again!"
        };
    }

    private List<string> GetStaticRecommendations()
    {
        return new List<string>
        {
            "Practice using @ references in Cursor for better context",
            "Try Cmd+K for inline code generation",
            "Explore the Tips & Tricks section for more insights",
            "Experiment with different AI models for various tasks",
            "Join the community to share and learn from others"
        };
    }

    private async Task<int> IncrementQuizAttemptForIpAsync(string ipHash)
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var key = $"ip:quiz:attempts:{ipHash}:{today}";
        
        try
        {
            var attemptsStr = await _redis.GetStringAsync(key);
            var attempts = int.TryParse(attemptsStr, out var attemptNumber) ? attemptNumber : 0;
            attemptNumber++;
            await _redis.SetAsync(key, attemptNumber.ToString(), TimeSpan.FromHours(24));
            return attemptNumber;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error incrementing quiz attempts for IP hash {IpHash}, returning 1", ipHash);
            return 1;
        }
    }

    private async Task TrackAnalyticsAsync(string sessionId, int score)
    {
        try
        {
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var hour = DateTime.UtcNow.ToString("yyyy-MM-dd-HH");
            
            await _redis.IncrementAsync($"analytics:daily:{today}:quizzesCompleted");
            await _redis.IncrementAsync($"analytics:hourly:{hour}:quizAttempts");
            
            _logger.LogInformation("Tracked quiz completion: {Score} for session {SessionId}", score, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error tracking quiz analytics for {SessionId}", sessionId);
            // Don't throw - analytics tracking should not break quiz functionality
        }
    }
}