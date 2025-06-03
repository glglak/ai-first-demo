using Azure.AI.OpenAI;
using Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AiFirstDemo.Infrastructure.OpenAI;

public class OpenAIService : IOpenAIService
{
    private readonly OpenAIClient _client;
    private readonly string _deploymentName;
    private readonly ILogger<OpenAIService> _logger;

    public OpenAIService(OpenAIClient client, IConfiguration configuration, ILogger<OpenAIService> logger)
    {
        _client = client;
        _deploymentName = configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4";
        _logger = logger;
    }

    public async Task<string> GenerateTextAsync(string prompt, string systemMessage = "", int maxTokens = 1000)
    {
        try
        {
            var messages = new List<ChatRequestMessage>();
            
            if (!string.IsNullOrEmpty(systemMessage))
            {
                messages.Add(new ChatRequestSystemMessage(systemMessage));
            }
            
            messages.Add(new ChatRequestUserMessage(prompt));

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = _deploymentName,
                MaxTokens = maxTokens,
                Temperature = 0.7f
            };

            foreach (var message in messages)
            {
                chatCompletionsOptions.Messages.Add(message);
            }

            var response = await _client.GetChatCompletionsAsync(chatCompletionsOptions);
            return response.Value.Choices[0].Message.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating text with prompt: {Prompt}", prompt);
            return "Sorry, I couldn't generate a response at this time.";
        }
    }

    public async Task<string> GenerateQuizExplanationAsync(string question, string userAnswer, string correctAnswer)
    {
        var systemMessage = @"You are an AI development expert helping developers learn about Cursor and AI-first development. 
        Provide clear, helpful explanations for quiz answers. Be encouraging and educational.";
        
        var prompt = $@"Question: {question}
        User's Answer: {userAnswer}
        Correct Answer: {correctAnswer}
        
        Please provide a brief, helpful explanation of why the correct answer is right and offer a practical tip related to this concept.";

        return await GenerateTextAsync(prompt, systemMessage, 300);
    }

    public async Task<List<string>> GenerateTipsAsync(string category, int count = 5)
    {
        var systemMessage = @"You are an expert in AI-first development and Cursor IDE. 
        Generate practical, actionable tips that developers can immediately apply.";
        
        var prompt = $@"Generate {count} practical tips about {category} for developers using Cursor and AI-first development. 
        Each tip should be:
        - Actionable and specific
        - 1-2 sentences long
        - Focused on real-world application
        
        Return the tips as a JSON array of strings.";

        try
        {
            var response = await GenerateTextAsync(prompt, systemMessage, 800);
            var tips = JsonSerializer.Deserialize<List<string>>(response);
            return tips ?? new List<string> { "Keep experimenting with AI-powered development!" };
        }
        catch
        {
            return new List<string> 
            {
                "Use Ctrl+K to quickly access Cursor's AI features",
                "Write clear, specific prompts for better AI assistance",
                "Leverage AI for code explanation and documentation",
                "Use AI to generate unit tests for your functions",
                "Ask AI to review your code for potential improvements"
            };
        }
    }

    public async Task<string> AnalyzeQuizPerformanceAsync(List<QuizAnalysisData> answers)
    {
        var systemMessage = @"You are an AI development mentor. Analyze quiz performance and provide personalized learning recommendations.";
        
        var correctCount = answers.Count(a => a.IsCorrect);
        var totalCount = answers.Count;
        var weakAreas = answers.Where(a => !a.IsCorrect).Select(a => a.Category).Distinct().ToList();
        
        var prompt = $@"The user scored {correctCount}/{totalCount} on the AI development quiz.
        Weak areas: {string.Join(", ", weakAreas)}
        
        Provide:
        1. A brief performance summary
        2. 2-3 specific learning recommendations
        3. Encouragement for continued learning
        
        Keep it positive and actionable (max 200 words).";

        return await GenerateTextAsync(prompt, systemMessage, 300);
    }

    public async Task<bool> IsContentAppropriateAsync(string content)
    {
        var systemMessage = "You are a content moderator. Determine if content is appropriate for a professional development environment.";
        var prompt = $"Is this content appropriate? Answer only 'YES' or 'NO': {content}";
        
        try
        {
            var response = await GenerateTextAsync(prompt, systemMessage, 10);
            return response.Trim().ToUpper().StartsWith("YES");
        }
        catch
        {
            return false; // Err on the side of caution
        }
    }

    public async Task<string> GenerateQuizHintAsync(string question, List<string> options, string category)
    {
        var systemMessage = @"You are an AI development mentor helping developers learn about Cursor and AI-first development. 
        Provide helpful hints for quiz questions without giving away the direct answer. 
        Focus on guiding the user's thinking process and providing context that helps them reason through the problem.";
        
        var prompt = $@"Question: {question}
        Category: {category}
        Options: {string.Join(", ", options)}
        
        Provide a helpful hint that:
        - Guides the user's thinking without revealing the answer
        - Gives context about the concept being tested
        - Helps them eliminate obviously wrong options
        - Is encouraging and educational
        - Is 1-2 sentences long
        
        Do NOT directly state which option is correct.";

        return await GenerateTextAsync(prompt, systemMessage, 200);
    }
}