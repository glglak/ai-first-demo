namespace AiFirstDemo.Features.TipsAndTricks.Models;

public record Tip(
    string Id,
    string Title,
    string Content,
    string Category,
    string Difficulty,
    List<string> Tags,
    DateTime CreatedAt,
    int Likes,
    bool IsAiGenerated,
    string? CreatedBy = null,
    string? CreatedByIpHash = null
);

public record CreateTipRequest(
    string Title,
    string Content,
    string Category,
    string Difficulty,
    List<string> Tags
);

public record GenerateTipsRequest(
    string Category,
    int Count = 5,
    string? Difficulty = null,
    string? Context = null
);

public record TipResponse(
    string Id,
    string Title,
    string Content,
    string Category,
    string Difficulty,
    List<string> Tags,
    DateTime CreatedAt,
    int Likes,
    bool IsLikedByUser,
    bool IsAiGenerated,
    string? CreatedBy = null
);

public record TipsCollectionResponse(
    List<TipResponse> Tips,
    int TotalCount,
    string Category,
    List<string> AvailableCategories
);