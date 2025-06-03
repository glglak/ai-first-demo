using AiFirstDemo.Features.Shared.Models;

namespace AiFirstDemo.Features.UserSessions;

public interface IUserSessionService
{
    Task<UserSession> CreateSessionAsync(CreateUserSessionRequest request);
    Task<UserSession?> GetSessionAsync(string sessionId);
    Task<bool> UpdateLastActivityAsync(string sessionId);
    Task<bool> MarkQuizCompletedAsync(string sessionId);
    Task<bool> SessionExistsForIpAsync(string ipAddress);
    Task<string?> GetLatestSessionForIpAsync(string ipAddress);
    Task<int> GetQuizAttemptsForIpAsync(string ipAddress);
    Task<bool> CanTakeQuizAsync(string ipAddress);
    Task<string> HashIpAddressAsync(string ipAddress);
    Task<int> CleanupCorruptedQuizCountersAsync();
}