using AiFirstDemo.Features.TipsAndTricks.Models;
using AiFirstDemo.Infrastructure.Redis;
using AiFirstDemo.Infrastructure.OpenAI;
using AiFirstDemo.Features.UserSessions;
using Microsoft.Extensions.Logging;

namespace AiFirstDemo.Features.TipsAndTricks;

public class TipsService : ITipsService
{
    private readonly IRedisService _redis;
    private readonly IOpenAIService _openAI;
    private readonly IUserSessionService _userSession;
    private readonly ILogger<TipsService> _logger;
    
    private const string TIPS_PREFIX = "tips:";
    private const string CATEGORY_PREFIX = "tips:category:";
    private const string USER_LIKES_PREFIX = "tips:likes:user:";
    private const string TIP_LIKES_PREFIX = "tips:likes:tip:";
    private const string CATEGORIES_KEY = "tips:categories";

    public TipsService(
        IRedisService redis,
        IOpenAIService openAI,
        IUserSessionService userSession,
        ILogger<TipsService> logger)
    {
        _redis = redis;
        _openAI = openAI;
        _userSession = userSession;
        _logger = logger;
    }

    public async Task<TipsCollectionResponse> GetTipsAsync(string? category = null, string? sessionId = null)
    {
        List<Tip> tips;
        
        if (string.IsNullOrEmpty(category))
        {
            // Get all tips from default categories
            tips = await GetTipsFromMultipleCategoriesAsync();
        }
        else
        {
            // Get tips from specific category
            var tipIds = await _redis.ListRangeAsync($"{CATEGORY_PREFIX}{category}");
            tips = new List<Tip>();
            
            foreach (var tipId in tipIds)
            {
                var tip = await _redis.GetAsync<Tip>($"{TIPS_PREFIX}{tipId}");
                if (tip != null)
                {
                    tips.Add(tip);
                }
            }
        }

        var categories = await GetCategoriesAsync();
        var userLikes = new HashSet<string>();
        
        if (!string.IsNullOrEmpty(sessionId))
        {
            var likedTips = await _redis.ListRangeAsync($"{USER_LIKES_PREFIX}{sessionId}");
            userLikes = likedTips.ToHashSet();
        }

        var tipResponses = tips.Select(tip => new TipResponse(
            Id: tip.Id,
            Title: tip.Title,
            Content: tip.Content,
            Category: tip.Category,
            Difficulty: tip.Difficulty,
            Tags: tip.Tags,
            CreatedAt: tip.CreatedAt,
            Likes: tip.Likes,
            IsLikedByUser: userLikes.Contains(tip.Id),
            IsAiGenerated: tip.IsAiGenerated
        )).OrderByDescending(t => t.Likes).ThenByDescending(t => t.CreatedAt).ToList();

        return new TipsCollectionResponse(
            Tips: tipResponses,
            TotalCount: tipResponses.Count,
            Category: category ?? "All",
            AvailableCategories: categories
        );
    }

    public async Task<Tip> CreateTipAsync(CreateTipRequest request, string sessionId)
    {
        var session = await _userSession.GetSessionAsync(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException("Invalid session");
        }

        var tipId = Guid.NewGuid().ToString();
        var tip = new Tip(
            Id: tipId,
            Title: request.Title,
            Content: request.Content,
            Category: request.Category,
            Difficulty: request.Difficulty,
            Tags: request.Tags,
            CreatedAt: DateTime.UtcNow,
            Likes: 0,
            IsAiGenerated: false
        );

        // Store the tip
        await _redis.SetAsync($"{TIPS_PREFIX}{tipId}", tip, TimeSpan.FromDays(365));
        
        // Add to category list
        await _redis.ListPushAsync($"{CATEGORY_PREFIX}{request.Category}", tipId);
        
        // Add category to categories list if not exists
        var categories = await GetCategoriesAsync();
        if (!categories.Contains(request.Category))
        {
            await _redis.ListPushAsync(CATEGORIES_KEY, request.Category);
        }

        _logger.LogInformation("Created tip {TipId} in category {Category}", tipId, request.Category);
        return tip;
    }

    public async Task<List<Tip>> GenerateAiTipsAsync(GenerateTipsRequest request)
    {
        var aiTips = await _openAI.GenerateTipsAsync(request.Category, request.Count);
        var tips = new List<Tip>();

        foreach (var aiTip in aiTips)
        {
            var tipId = Guid.NewGuid().ToString();
            var tip = new Tip(
                Id: tipId,
                Title: ExtractTitleFromTip(aiTip),
                Content: aiTip,
                Category: request.Category,
                Difficulty: request.Difficulty ?? "Medium",
                Tags: ExtractTagsFromCategory(request.Category),
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: true
            );

            // Store the tip
            await _redis.SetAsync($"{TIPS_PREFIX}{tipId}", tip, TimeSpan.FromDays(30));
            
            // Add to category list
            await _redis.ListPushAsync($"{CATEGORY_PREFIX}{request.Category}", tipId);
            
            tips.Add(tip);
        }

        _logger.LogInformation("Generated {Count} AI tips for category {Category}", tips.Count, request.Category);
        return tips;
    }

    public async Task<bool> LikeTipAsync(string tipId, string sessionId)
    {
        var session = await _userSession.GetSessionAsync(sessionId);
        if (session == null)
        {
            return false;
        }

        var tip = await _redis.GetAsync<Tip>($"{TIPS_PREFIX}{tipId}");
        if (tip == null)
        {
            return false;
        }

        // Check if already liked
        var userLikes = await _redis.ListRangeAsync($"{USER_LIKES_PREFIX}{sessionId}");
        if (userLikes.Contains(tipId))
        {
            return false; // Already liked
        }

        // Add like
        await _redis.ListPushAsync($"{USER_LIKES_PREFIX}{sessionId}", tipId);
        
        // Update tip likes count
        var updatedTip = tip with { Likes = tip.Likes + 1 };
        await _redis.SetAsync($"{TIPS_PREFIX}{tipId}", updatedTip, TimeSpan.FromDays(365));

        _logger.LogInformation("User {SessionId} liked tip {TipId}", sessionId, tipId);
        return true;
    }

    public async Task<bool> UnlikeTipAsync(string tipId, string sessionId)
    {
        var session = await _userSession.GetSessionAsync(sessionId);
        if (session == null)
        {
            return false;
        }

        var tip = await _redis.GetAsync<Tip>($"{TIPS_PREFIX}{tipId}");
        if (tip == null)
        {
            return false;
        }

        // Remove from user likes (simplified - would need better implementation)
        // For demo purposes, we'll just decrement the count
        var updatedTip = tip with { Likes = Math.Max(0, tip.Likes - 1) };
        await _redis.SetAsync($"{TIPS_PREFIX}{tipId}", updatedTip, TimeSpan.FromDays(365));

        _logger.LogInformation("User {SessionId} unliked tip {TipId}", sessionId, tipId);
        return true;
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        var categories = await _redis.ListRangeAsync(CATEGORIES_KEY);
        if (!categories.Any())
        {
            // Initialize with default categories
            var defaultCategories = new List<string>
            {
                "Cursor Basics",
                "AI Integration",
                "Best Practices",
                "Advanced Features",
                "Productivity Tips",
                "Debugging",
                "Code Generation"
            };

            foreach (var category in defaultCategories)
            {
                await _redis.ListPushAsync(CATEGORIES_KEY, category);
            }

            return defaultCategories;
        }

        return categories;
    }

    public async Task<Tip?> GetTipAsync(string tipId)
    {
        return await _redis.GetAsync<Tip>($"{TIPS_PREFIX}{tipId}");
    }

    private async Task<List<Tip>> GetTipsFromMultipleCategoriesAsync()
    {
        var categories = await GetCategoriesAsync();
        var allTips = new List<Tip>();

        foreach (var category in categories)
        {
            var tipIds = await _redis.ListRangeAsync($"{CATEGORY_PREFIX}{category}");
            foreach (var tipId in tipIds.Take(3)) // Limit per category
            {
                var tip = await _redis.GetAsync<Tip>($"{TIPS_PREFIX}{tipId}");
                if (tip != null)
                {
                    allTips.Add(tip);
                }
            }
        }

        return allTips;
    }

    private string ExtractTitleFromTip(string tipContent)
    {
        // Extract first sentence or first 50 characters as title
        var sentences = tipContent.Split('.', '!', '?');
        var title = sentences[0].Trim();
        
        if (title.Length > 50)
        {
            title = title.Substring(0, 47) + "...";
        }

        return title;
    }

    private List<string> ExtractTagsFromCategory(string category)
    {
        return category switch
        {
            "Cursor Basics" => new List<string> { "cursor", "basics", "getting-started" },
            "AI Integration" => new List<string> { "ai", "integration", "models" },
            "Best Practices" => new List<string> { "best-practices", "workflow", "efficiency" },
            "Advanced Features" => new List<string> { "advanced", "features", "power-user" },
            "Productivity Tips" => new List<string> { "productivity", "tips", "shortcuts" },
            "Debugging" => new List<string> { "debugging", "troubleshooting", "errors" },
            "Code Generation" => new List<string> { "code-generation", "ai-assistance", "automation" },
            _ => new List<string> { "general", "tips" }
        };
    }
}