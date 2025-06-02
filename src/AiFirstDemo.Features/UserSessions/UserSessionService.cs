using AiFirstDemo.Features.Shared.Models;
using AiFirstDemo.Infrastructure.Redis;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AiFirstDemo.Features.UserSessions;

public class UserSessionService : IUserSessionService
{
    private readonly IRedisService _redis;
    private readonly ILogger<UserSessionService> _logger;
    private const string SESSION_KEY_PREFIX = "user:session:";
    private const string IP_SESSION_PREFIX = "ip:session:";

    public UserSessionService(IRedisService redis, ILogger<UserSessionService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<UserSession> CreateSessionAsync(CreateUserSessionRequest request)
    {
        var sessionId = Guid.NewGuid().ToString();
        var ipHash = await HashIpAddressAsync(request.IpAddress);
        
        var session = new UserSession(
            SessionId: sessionId,
            Name: request.Name,
            IpHash: ipHash,
            CreatedAt: DateTime.UtcNow,
            LastActivity: DateTime.UtcNow,
            HasCompletedQuiz: false
        );

        // Store session data
        await _redis.SetAsync($"{SESSION_KEY_PREFIX}{sessionId}", session, TimeSpan.FromHours(24));
        
        // Track IP to prevent multiple sessions
        await _redis.SetAsync($"{IP_SESSION_PREFIX}{ipHash}", sessionId, TimeSpan.FromHours(24));
        
        _logger.LogInformation("Created session {SessionId} for user {Name}", sessionId, request.Name);
        
        return session;
    }

    public async Task<UserSession?> GetSessionAsync(string sessionId)
    {
        var session = await _redis.GetAsync<UserSession>($"{SESSION_KEY_PREFIX}{sessionId}");
        
        if (session != null)
        {
            await UpdateLastActivityAsync(sessionId);
        }
        
        return session;
    }

    public async Task<bool> UpdateLastActivityAsync(string sessionId)
    {
        var session = await _redis.GetAsync<UserSession>($"{SESSION_KEY_PREFIX}{sessionId}");
        if (session == null) return false;

        var updatedSession = session with { LastActivity = DateTime.UtcNow };
        return await _redis.SetAsync($"{SESSION_KEY_PREFIX}{sessionId}", updatedSession, TimeSpan.FromHours(24));
    }

    public async Task<bool> MarkQuizCompletedAsync(string sessionId)
    {
        var session = await _redis.GetAsync<UserSession>($"{SESSION_KEY_PREFIX}{sessionId}");
        if (session == null) return false;

        var updatedSession = session with { HasCompletedQuiz = true, LastActivity = DateTime.UtcNow };
        return await _redis.SetAsync($"{SESSION_KEY_PREFIX}{sessionId}", updatedSession, TimeSpan.FromHours(24));
    }

    public async Task<bool> SessionExistsForIpAsync(string ipAddress)
    {
        var ipHash = await HashIpAddressAsync(ipAddress);
        return await _redis.ExistsAsync($"{IP_SESSION_PREFIX}{ipHash}");
    }

    public async Task<string> HashIpAddressAsync(string ipAddress)
    {
        // Simple hash for demo purposes - in production, consider more sophisticated approach
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(ipAddress + "salt123"));
        return Convert.ToHexString(hashedBytes)[..16]; // Take first 16 characters
    }
}