using AiFirstDemo.Features.TipsAndTricks.Models;

namespace AiFirstDemo.Features.TipsAndTricks;

public interface ITipsService
{
    Task<TipsCollectionResponse> GetTipsAsync(string? category = null, string? sessionId = null);
    Task<Tip> CreateTipAsync(CreateTipRequest request, string sessionId);
    Task<List<Tip>> GenerateAiTipsAsync(GenerateTipsRequest request);
    Task<bool> LikeTipAsync(string tipId, string sessionId);
    Task<bool> UnlikeTipAsync(string tipId, string sessionId);
    Task<List<string>> GetCategoriesAsync();
    Task<Tip?> GetTipAsync(string tipId);
    Task EnsureTipsSeededAsync();
    Task ForceReseedAsync();
}