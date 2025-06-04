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
    private const string IP_QUIZ_ATTEMPTS_PREFIX = "ip:quiz:attempts:";
    private const int MAX_QUIZ_ATTEMPTS_PER_DAY = 3;

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
        var sessionKey = $"{SESSION_KEY_PREFIX}{sessionId}";
        _logger.LogInformation("Storing session with key: {Key}", sessionKey);
        await _redis.SetAsync(sessionKey, session, TimeSpan.FromHours(24));
        
        // Verify it was stored immediately
        var verifySession = await _redis.GetAsync<UserSession>(sessionKey);
        if (verifySession != null)
        {
            _logger.LogInformation("Session verified immediately after creation: {SessionId}", sessionId);
        }
        else
        {
            _logger.LogError("SESSION CREATION FAILED: Session not found immediately after creation: {SessionId} with key {Key}", sessionId, sessionKey);
        }
        
        // Store the latest session for this IP (allow multiple sessions but track the latest)
        await _redis.SetAsync($"{IP_SESSION_PREFIX}{ipHash}", sessionId, TimeSpan.FromHours(24));
        
        // Store partially masked IP for analytics display (e.g., "192.168.1.xxx")
        var maskedIp = MaskIpAddress(request.IpAddress);
        await _redis.SetAsync($"session:ip:display:{sessionId}", maskedIp, TimeSpan.FromHours(24));
        
        // Track analytics
        await TrackSessionActivityAsync(sessionId, "session_created");
        
        _logger.LogInformation("Created session {SessionId} for user {Name} with IP hash {IpHash}", 
            sessionId, request.Name, ipHash);
        
        return session;
    }

    public async Task<UserSession?> GetSessionAsync(string sessionId)
    {
        var sessionKey = $"{SESSION_KEY_PREFIX}{sessionId}";
        _logger.LogInformation("Attempting to retrieve session with key: {Key}", sessionKey);
        
        var session = await _redis.GetAsync<UserSession>(sessionKey);
        
        if (session != null)
        {
            _logger.LogInformation("Session found successfully: {SessionId}", sessionId);
            await UpdateLastActivityAsync(sessionId);
        }
        else
        {
            _logger.LogWarning("Session NOT FOUND in Redis: {SessionId} with key {Key}", sessionId, sessionKey);
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

        // Increment quiz attempts for this IP
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var attemptKey = $"{IP_QUIZ_ATTEMPTS_PREFIX}{session.IpHash}:{today}";
        
        try
        {
            // Increment the counter (this returns the new value as a number)
            var currentAttempts = await _redis.IncrementAsync(attemptKey);
            
            // Set expiry for the key if it's the first increment (Redis increment creates the key if it doesn't exist)
            if (currentAttempts == 1)
            {
                // Set expiry on the raw integer key (not as JSON)
                await _redis.SetExpiryAsync(attemptKey, TimeSpan.FromDays(1));
            }

            var updatedSession = session with { HasCompletedQuiz = true, LastActivity = DateTime.UtcNow };
            return await _redis.SetAsync($"{SESSION_KEY_PREFIX}{sessionId}", updatedSession, TimeSpan.FromHours(24));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing quiz attempts for key {Key}. Attempting to clean up corrupted data.", attemptKey);
            
            // Clean up potentially corrupted key and retry
            await _redis.DeleteAsync(attemptKey);
            
            try
            {
                // Retry the increment after cleanup
                var currentAttempts = await _redis.IncrementAsync(attemptKey);
                await _redis.SetExpiryAsync(attemptKey, TimeSpan.FromDays(1));

        var updatedSession = session with { HasCompletedQuiz = true, LastActivity = DateTime.UtcNow };
        return await _redis.SetAsync($"{SESSION_KEY_PREFIX}{sessionId}", updatedSession, TimeSpan.FromHours(24));
            }
            catch (Exception retryEx)
            {
                _logger.LogError(retryEx, "Failed to increment quiz attempts even after cleanup for key {Key}", attemptKey);
                
                // Still update the session even if counter fails
                var updatedSession = session with { HasCompletedQuiz = true, LastActivity = DateTime.UtcNow };
                return await _redis.SetAsync($"{SESSION_KEY_PREFIX}{sessionId}", updatedSession, TimeSpan.FromHours(24));
            }
        }
    }

    public async Task<bool> SessionExistsForIpAsync(string ipAddress)
    {
        var ipHash = await HashIpAddressAsync(ipAddress);
        return await _redis.ExistsAsync($"{IP_SESSION_PREFIX}{ipHash}");
    }

    public async Task<string?> GetLatestSessionForIpAsync(string ipAddress)
    {
        var ipHash = await HashIpAddressAsync(ipAddress);
        return await _redis.GetStringAsync($"{IP_SESSION_PREFIX}{ipHash}");
    }

    public async Task<int> GetQuizAttemptsForIpAsync(string ipAddress)
    {
        var ipHash = await HashIpAddressAsync(ipAddress);
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var key = $"{IP_QUIZ_ATTEMPTS_PREFIX}{ipHash}:{today}";
        
        try
        {
            var attemptsStr = await _redis.GetStringAsync(key);
            return int.TryParse(attemptsStr, out var attempts) ? attempts : 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting quiz attempts for IP hash {IpHash}, returning 0. This might be due to Redis timeout or connectivity issues.", ipHash);
            return 0; // Fail gracefully - assume no attempts if we can't check
        }
    }

    public async Task<bool> CanTakeQuizAsync(string ipAddress)
    {
        try
        {
            var attempts = await GetQuizAttemptsForIpAsync(ipAddress);
            return attempts < MAX_QUIZ_ATTEMPTS_PER_DAY;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking quiz eligibility for IP {IpAddress}, allowing quiz attempt", ipAddress);
            return true; // Fail open - allow quiz if we can't check
        }
    }

    public Task<string> HashIpAddressAsync(string ipAddress)
    {
        // Simple hash for demo purposes - in production, consider more sophisticated approach
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(ipAddress + "salt123"));
        return Task.FromResult(Convert.ToHexString(hashedBytes)[..16]); // Take first 16 characters
    }

    /// <summary>
    /// Cleans up any potentially corrupted quiz attempt counters in Redis.
    /// This method can be called to fix data type conflicts.
    /// </summary>
    public async Task<int> CleanupCorruptedQuizCountersAsync()
    {
        try
        {
            var pattern = $"{IP_QUIZ_ATTEMPTS_PREFIX}*";
            var keys = await _redis.GetKeysAsync(pattern);
            
            var cleanedCount = 0;
            foreach (var key in keys)
            {
                try
                {
                    // Try to get as string - if this fails, the key is corrupted
                    var value = await _redis.GetStringAsync(key);
                    if (value != null && !int.TryParse(value, out _))
                    {
                        // Key exists but contains non-integer data - delete it
                        await _redis.DeleteAsync(key);
                        cleanedCount++;
                        _logger.LogInformation("Cleaned up corrupted quiz counter key: {Key}", key);
                    }
                }
                catch (Exception ex)
                {
                    // If we can't read the key, it's likely corrupted - delete it
                    _logger.LogWarning(ex, "Found corrupted key {Key}, deleting it", key);
                    await _redis.DeleteAsync(key);
                    cleanedCount++;
                }
            }
            
            _logger.LogInformation("Cleaned up {Count} corrupted quiz counter keys", cleanedCount);
            return cleanedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during quiz counter cleanup");
            return 0;
        }
    }

    public async Task<string?> GetDisplayIpAsync(string sessionId)
    {
        try
        {
            var maskedIp = await _redis.GetStringAsync($"session:ip:display:{sessionId}");
            return maskedIp ?? "unknown";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting display IP for session {SessionId}", sessionId);
            return "unknown";
        }
    }

    private string MaskIpAddress(string ipAddress)
    {
        try
        {
            // For IPv4: show first two octets, mask the last two (e.g., "192.168.xxx.xxx")
            if (ipAddress.Contains('.'))
            {
                var parts = ipAddress.Split('.');
                if (parts.Length == 4)
                {
                    return $"{parts[0]}.{parts[1]}.xxx.xxx";
                }
            }
            
            // For IPv6 or other formats: show first part and mask the rest
            if (ipAddress.Contains(':'))
            {
                var parts = ipAddress.Split(':');
                if (parts.Length > 2)
                {
                    return $"{parts[0]}:{parts[1]}:xxxx::xxxx";
                }
            }
            
            // Fallback: show first 4 characters
            return ipAddress.Length > 4 ? ipAddress.Substring(0, 4) + "xxx" : "xxx";
        }
        catch
        {
            return "xxx.xxx.xxx.xxx";
        }
    }

    private async Task TrackSessionActivityAsync(string sessionId, string activity)
    {
        try
        {
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var hour = DateTime.UtcNow.ToString("yyyy-MM-dd-HH");
            
            // Track daily totals
            await _redis.IncrementAsync($"analytics:daily:{today}:totalSessions");
            
            // Track hourly activity
            await _redis.IncrementAsync($"analytics:hourly:{hour}:sessions");
            
            // Track user activity
            await _redis.HashSetAsync($"analytics:user:activity:{sessionId}", "lastActivity", DateTime.UtcNow.ToString());
            await _redis.HashSetAsync($"analytics:user:activity:{sessionId}", "activity", activity);
            
            _logger.LogInformation("Tracked activity: {Activity} for session {SessionId}", activity, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error tracking session activity for {SessionId}", sessionId);
            // Don't throw - analytics tracking should not break session creation
        }
    }
}