namespace AiFirstDemo.Infrastructure.OpenAI;

public interface IOpenAIService
{
    Task<string> GenerateTextAsync(string prompt, string systemMessage = "", int maxTokens = 1000);
    Task<string> GenerateQuizExplanationAsync(string question, string userAnswer, string correctAnswer);
    Task<List<string>> GenerateTipsAsync(string category, int count = 5);
    Task<string> AnalyzeQuizPerformanceAsync(List<QuizAnalysisData> answers);
    Task<bool> IsContentAppropriateAsync(string content);
}

public record QuizAnalysisData(
    string Question,
    string UserAnswer,
    string CorrectAnswer,
    bool IsCorrect,
    string Category
);