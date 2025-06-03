namespace AiFirstDemo.Features.Quiz.Models;

public record QuizQuestion(
    string Id,
    string Text,
    List<string> Options,
    string CorrectAnswer,
    string Category,
    string Difficulty
);

public record QuizSubmissionRequest(
    string SessionId,
    List<QuizAnswerSubmission> Answers
);

public record QuizAnswerSubmission(
    string QuestionId,
    string SelectedAnswer
);

public record QuizAnswer(
    string QuestionId,
    string SelectedAnswer,
    bool IsCorrect,
    DateTime AnsweredAt
);

public record QuizResultResponse(
    int Score,
    int TotalQuestions,
    double Percentage,
    List<QuestionResult> Results,
    string AiAnalysis,
    List<string> Recommendations
);

public record QuestionResult(
    string QuestionText,
    string SelectedAnswer,
    string CorrectAnswer,
    bool IsCorrect,
    string Explanation
);

public record QuizAttempt(
    string SessionId,
    List<QuizAnswer> Answers,
    int Score,
    DateTime CompletedAt,
    TimeSpan Duration,
    string AiAnalysis
);

public interface IQuizService
{
    Task<List<QuizQuestion>> GetQuizQuestionsAsync();
    Task<List<QuizQuestion>> GenerateRandomQuestionsAsync(int count = 10);
    Task<QuizResultResponse> SubmitQuizAsync(QuizSubmissionRequest request);
    Task<QuizAttempt?> GetQuizAttemptAsync(string sessionId);
    Task<string?> GetQuestionHintAsync(string questionId);
} 