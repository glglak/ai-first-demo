namespace AiFirstDemo.Features.Shared.Models;

public record UserSession(
    string SessionId,
    string Name,
    string IpHash,
    DateTime CreatedAt,
    DateTime LastActivity,
    bool HasCompletedQuiz
);

public record CreateUserSessionRequest(
    string Name,
    string IpAddress
);

public record UserSessionResponse(
    string SessionId,
    string Name,
    DateTime CreatedAt,
    bool HasCompletedQuiz
);