using AiFirstDemo.Features.Analytics.Models;
using AiFirstDemo.Infrastructure.Redis;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using AiFirstDemo.Features.Quiz.Models;
using AiFirstDemo.Features.Shared.Models;
using AiFirstDemo.Features.TipsAndTricks.Models;
using AiFirstDemo.Features.SpaceshipGame.Models;
using System.Security.Cryptography;
using System.Text;

namespace AiFirstDemo.Features.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private readonly IRedisService _redis;
    private readonly ILogger<AnalyticsService> _logger;
    
    private const string ANALYTICS_PREFIX = "analytics:";
    private const string DAILY_STATS = "daily:";
    private const string HOURLY_STATS = "hourly:";
    private const string USER_ACTIVITY = "user:activity:";

    public AnalyticsService(IRedisService redis, ILogger<AnalyticsService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<DashboardData> GetDashboardDataAsync()
    {
        try
        {
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            
            // Get basic stats with safe parsing
            var totalSessionsStr = await _redis.HashGetAsync($"{ANALYTICS_PREFIX}{DAILY_STATS}{today}", "totalSessions");
            var quizzesCompletedStr = await _redis.HashGetAsync($"{ANALYTICS_PREFIX}{DAILY_STATS}{today}", "quizzesCompleted");
            var gamesPlayedStr = await _redis.HashGetAsync($"{ANALYTICS_PREFIX}{DAILY_STATS}{today}", "gamesPlayed");
            
            var totalSessions = int.TryParse(totalSessionsStr, out var ts) ? ts : 0;
            var quizzesCompleted = int.TryParse(quizzesCompletedStr, out var qc) ? qc : 0;
            var gamesPlayed = int.TryParse(gamesPlayedStr, out var gp) ? gp : 0;
            
            // Get active sessions (simplified - last 30 minutes)
            var activeSessionsCount = 0; // Would implement real-time tracking
            
            // Get hourly activity for the last 24 hours
            var hourlyActivity = await GetHourlyActivityAsync(DateTime.UtcNow);
            
            return new DashboardData(
                TotalSessions: totalSessions,
                ActiveSessionsNow: activeSessionsCount,
                QuizzesCompleted: quizzesCompleted,
                GamesPlayed: gamesPlayed,
                AverageQuizScore: 0, // Would calculate from quiz data
                TopGameScore: 0, // Would get from game leaderboard
                HourlyActivity: hourlyActivity ?? new List<HourlyActivity>(),
                QuizPerformance: new List<CategoryPerformance>(),
                PopularTips: new List<PopularTip>()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard data");
            
            // Return empty dashboard data instead of throwing
            return new DashboardData(
                TotalSessions: 0,
                ActiveSessionsNow: 0,
                QuizzesCompleted: 0,
                GamesPlayed: 0,
                AverageQuizScore: 0,
                TopGameScore: 0,
                HourlyActivity: new List<HourlyActivity>(),
                QuizPerformance: new List<CategoryPerformance>(),
                PopularTips: new List<PopularTip>()
            );
        }
    }

    public Task<List<UserActivity>> GetActiveUsersAsync()
    {
        // Implementation would track active users
        return Task.FromResult(new List<UserActivity>());
    }

    public async Task TrackSessionActivityAsync(string sessionId, string activity)
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var hour = DateTime.UtcNow.ToString("yyyy-MM-dd-HH");
        
        // Track daily totals
        await _redis.IncrementAsync($"{ANALYTICS_PREFIX}{DAILY_STATS}{today}:totalSessions");
        
        // Track hourly activity
        await _redis.IncrementAsync($"{ANALYTICS_PREFIX}{HOURLY_STATS}{hour}:sessions");
        
        // Track user activity
        await _redis.HashSetAsync($"{ANALYTICS_PREFIX}{USER_ACTIVITY}{sessionId}", "lastActivity", DateTime.UtcNow.ToString());
        await _redis.HashSetAsync($"{ANALYTICS_PREFIX}{USER_ACTIVITY}{sessionId}", "activity", activity);
        
        _logger.LogInformation("Tracked activity: {Activity} for session {SessionId}", activity, sessionId);
    }

    public async Task TrackQuizCompletionAsync(string sessionId, int score)
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var hour = DateTime.UtcNow.ToString("yyyy-MM-dd-HH");
        
        await _redis.IncrementAsync($"{ANALYTICS_PREFIX}{DAILY_STATS}{today}:quizzesCompleted");
        await _redis.IncrementAsync($"{ANALYTICS_PREFIX}{HOURLY_STATS}{hour}:quizAttempts");
        
        _logger.LogInformation("Tracked quiz completion: {Score} for session {SessionId}", score, sessionId);
    }

    public async Task TrackGamePlayAsync(string sessionId, int score)
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var hour = DateTime.UtcNow.ToString("yyyy-MM-dd-HH");
        
        await _redis.IncrementAsync($"{ANALYTICS_PREFIX}{DAILY_STATS}{today}:gamesPlayed");
        await _redis.IncrementAsync($"{ANALYTICS_PREFIX}{HOURLY_STATS}{hour}:gamePlays");
        
        _logger.LogInformation("Tracked game play: {Score} for session {SessionId}", score, sessionId);
    }

    public async Task<List<HourlyActivity>> GetHourlyActivityAsync(DateTime date)
    {
        try
        {
            var activities = new List<HourlyActivity>();
            
            for (int i = 0; i < 24; i++)
            {
                var hour = date.AddHours(-i).ToString("yyyy-MM-dd-HH");
                var sessionsStr = await _redis.HashGetAsync($"{ANALYTICS_PREFIX}{HOURLY_STATS}{hour}", "sessions");
                var quizAttemptsStr = await _redis.HashGetAsync($"{ANALYTICS_PREFIX}{HOURLY_STATS}{hour}", "quizAttempts");
                var gamePlaysStr = await _redis.HashGetAsync($"{ANALYTICS_PREFIX}{HOURLY_STATS}{hour}", "gamePlays");
                
                var sessions = int.TryParse(sessionsStr, out var s) ? s : 0;
                var quizAttempts = int.TryParse(quizAttemptsStr, out var qa) ? qa : 0;
                var gamePlays = int.TryParse(gamePlaysStr, out var gp) ? gp : 0;
                
                activities.Add(new HourlyActivity(
                    Hour: date.AddHours(-i),
                    Sessions: sessions,
                    QuizAttempts: quizAttempts,
                    GamePlays: gamePlays
                ));
            }
            
            return activities.OrderBy(a => a.Hour).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hourly activity for {Date}", date);
            return new List<HourlyActivity>();
        }
    }

    public async Task<UnifiedLeaderboardResponse> GetUnifiedLeaderboardAsync()
    {
        try
        {
            // Use the separate methods for better performance - get top 10 for unified view
            var quizParticipants = await GetQuizParticipantsAsync(10, 0);
            var gameParticipants = await GetGameParticipantsAsync(10, 0);
            var tipsContributors = await GetTipsContributorsAsync(10, 0);

            return new UnifiedLeaderboardResponse(
                QuizParticipants: quizParticipants,
                GameParticipants: gameParticipants,
                TipsContributors: tipsContributors
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unified leaderboard");
            return new UnifiedLeaderboardResponse(
                QuizParticipants: new List<UnifiedParticipant>(),
                GameParticipants: new List<UnifiedParticipant>(),
                TipsContributors: new List<UnifiedParticipant>()
            );
        }
    }

    public async Task<List<UnifiedParticipant>> GetQuizParticipantsAsync(int limit = 10, int offset = 0)
    {
        try
        {
            _logger.LogInformation("Getting quiz participants with limit {Limit}, offset {Offset}...", limit, offset);
            
            // Clear any old corrupted cache on first call
            if (offset == 0)
            {
                await ClearOldCacheAsync();
            }
            
            // Use server-side pagination - get sorted keys first
            var sortedAttempts = await GetSortedQuizAttemptsAsync();
            
            // Apply pagination to the sorted list
            var paginatedAttempts = sortedAttempts
                .Skip(offset)
                .Take(limit)
                .ToList();
            
            // Now get the detailed data only for the items we need
            var results = new List<UnifiedParticipant>();
            
            foreach (var attemptInfo in paginatedAttempts)
            {
                try
                {
                    var session = await _redis.GetAsync<UserSession>($"user:session:{attemptInfo.SessionId}");
                    if (session != null)
                    {
                        results.Add(new UnifiedParticipant(
                            Name: session.Name,
                            IpHash: "hidden", // Not needed for UI
                            Score: attemptInfo.Score,
                            Activity: attemptInfo.Activity,
                            LastActive: attemptInfo.CompletedAt
                        ));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing attempt for session {SessionId}", attemptInfo.SessionId);
                }
            }
            
            _logger.LogInformation("Returning {Count} quiz participants (offset: {Offset})", results.Count, offset);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quiz participants");
            return new List<UnifiedParticipant>();
        }
    }

    private async Task<List<QuizAttemptInfo>> GetSortedQuizAttemptsAsync()
    {
        var cacheKey = "analytics:quiz:sorted:v2"; // v2 to avoid old cache
        var cacheExpiry = TimeSpan.FromMinutes(5);
        
        // Try cache first
        var cachedData = await _redis.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            try
            {
                var cached = System.Text.Json.JsonSerializer.Deserialize<List<QuizAttemptInfo>>(cachedData);
                if (cached != null)
                {
                    _logger.LogInformation("Using cached sorted quiz attempts: {Count} items", cached.Count);
                    return cached;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize cached quiz attempts, rebuilding...");
            }
        }
        
        // Build fresh sorted list
        _logger.LogInformation("Building fresh sorted quiz attempts...");
        var attempts = await BuildSortedQuizAttemptsAsync();
        
        // Cache the sorted list
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(attempts);
            await _redis.SetAsync(cacheKey, json, cacheExpiry);
            _logger.LogInformation("Cached {Count} sorted quiz attempts", attempts.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache sorted quiz attempts");
        }
        
        return attempts;
    }
    
    private async Task<List<QuizAttemptInfo>> BuildSortedQuizAttemptsAsync()
    {
        var attempts = new List<QuizAttemptInfo>();
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var processedAttempts = new HashSet<string>();
        
        try
        {
            // Get keys
            var ipAttemptKeys = await _redis.GetKeysAsync("quiz:ip:attempt:*");
            var regularAttemptKeys = await _redis.GetKeysAsync("quiz:attempt:*");
            
            _logger.LogInformation("Processing {IpCount} IP attempts and {RegularCount} regular attempts", 
                ipAttemptKeys.Count, regularAttemptKeys.Count);
            
            // Process IP-based attempts
            foreach (var ipKey in ipAttemptKeys)
            {
                try
                {
                    var attempt = await _redis.GetAsync<QuizAttempt>(ipKey);
                    if (attempt != null && attempt.CompletedAt >= thirtyDaysAgo)
                    {
                        var keyParts = ipKey.Split(':');
                        if (keyParts.Length >= 6)
                        {
                            var sessionId = keyParts[5];
                            var attemptNumber = keyParts[4];
                            var attemptKey = $"{sessionId}:{attempt.CompletedAt:yyyy-MM-dd-HH-mm-ss}:{attempt.Score}";
                            
                            if (!processedAttempts.Contains(attemptKey))
                            {
                                attempts.Add(new QuizAttemptInfo(
                                    SessionId: sessionId,
                                    Score: attempt.Score,
                                    CompletedAt: attempt.CompletedAt,
                                    Activity: $"Quiz Completed - Try #{attemptNumber}"
                                ));
                                processedAttempts.Add(attemptKey);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing IP attempt {Key}", ipKey);
                }
            }
            
            // Process regular attempts
            foreach (var key in regularAttemptKeys)
            {
                try
                {
                    var attempt = await _redis.GetAsync<QuizAttempt>(key);
                    if (attempt != null && attempt.CompletedAt >= thirtyDaysAgo)
                    {
                        var sessionId = key.Replace("quiz:attempt:", "");
                        var attemptKey = $"{sessionId}:{attempt.CompletedAt:yyyy-MM-dd-HH-mm-ss}:{attempt.Score}";
                        
                        if (!processedAttempts.Contains(attemptKey))
                        {
                            attempts.Add(new QuizAttemptInfo(
                                SessionId: sessionId,
                                Score: attempt.Score,
                                CompletedAt: attempt.CompletedAt,
                                Activity: "Quiz Completed - Try #1"
                            ));
                            processedAttempts.Add(attemptKey);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing regular attempt {Key}", key);
                }
            }
            
            // Sort by score descending, then by date descending
            return attempts
                .OrderByDescending(a => a.Score)
                .ThenByDescending(a => a.CompletedAt)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building sorted quiz attempts");
            return new List<QuizAttemptInfo>();
        }
    }
    
    private async Task ClearOldCacheAsync()
    {
        try
        {
            // Clear old cache keys that might have serialization issues
            var oldKeys = new[]
            {
                "analytics:quiz:leaderboard",
                "analytics:quiz:sorted",
                "analytics:quiz:sorted:v1"
            };
            
            foreach (var key in oldKeys)
            {
                await _redis.DeleteAsync(key);
            }
            
            _logger.LogInformation("Cleared old analytics cache keys");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error clearing old cache");
        }
    }

    // Simple data transfer record for caching
    private record QuizAttemptInfo(string SessionId, int Score, DateTime CompletedAt, string Activity);

    public async Task<List<UnifiedParticipant>> GetGameParticipantsAsync(int limit = 10, int offset = 0)
    {
        try
        {
            var gameParticipants = new List<UnifiedParticipant>();
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30); // Extended to 30 days to match game score retention
            
            _logger.LogInformation("Getting game participants from last 30 days with limit {Limit}, offset {Offset}...", limit, offset);
            
            // Get game participants from leaderboard - but we need to filter by date
            var gameLeaderboard = await _redis.SortedSetRangeWithScoresAsync("game:leaderboard:alltime", 0, -1, true);
            
            foreach (var entry in gameLeaderboard)
            {
                try
                {
                    var playerData = entry.Member.Split(':');
                    if (playerData.Length >= 2)
                    {
                        var playerName = playerData[0];
                        var scoreId = playerData[1];
                        
                        // Get the score record to check the date and get session info
                        var gameScore = await _redis.GetAsync<GameScore>($"game:score:{scoreId}");
                        if (gameScore != null && gameScore.AchievedAt >= thirtyDaysAgo)
                        {
                            // Get session data to get IP information
                            var session = await _redis.GetAsync<UserSession>($"user:session:{gameScore.SessionId}");
                            var displayIp = session != null ? await GetDisplayIpAsync(gameScore.SessionId) : "unknown";
                            
                            gameParticipants.Add(new UnifiedParticipant(
                                Name: playerName,
                                IpHash: displayIp ?? "unknown",
                                Score: (int)entry.Score,
                                Activity: $"Game High Score (Level {gameScore.Level})",
                                LastActive: gameScore.AchievedAt
                            ));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing game entry {Entry}", entry.Member);
                }
            }
            
            // Sort by score descending and apply pagination
            var paginatedResults = gameParticipants
                .OrderByDescending(p => p.Score)
                .ThenByDescending(p => p.LastActive)
                .Skip(offset)
                .Take(limit)
                .ToList();
            
            _logger.LogInformation("Found {Total} game participants in last 30 days, returning {Count} with pagination", gameParticipants.Count, paginatedResults.Count);
            return paginatedResults;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting game participants");
            return new List<UnifiedParticipant>();
        }
    }

    public async Task<List<UnifiedParticipant>> GetTipsContributorsAsync(int limit = 10, int offset = 0)
    {
        try
        {
            var tipsContributors = new List<UnifiedParticipant>();
            
            _logger.LogInformation("Getting tips contributors with limit {Limit}, offset {Offset}...", limit, offset);
            
            // Get all tips from Redis
            var tipKeys = await _redis.GetKeysAsync("tips:*");
            var tipDataKeys = tipKeys.Where(k => !k.Contains("category:") && !k.Contains("likes:") && !k.Contains("categories")).ToList();
            
            foreach (var key in tipDataKeys)
            {
                try
                {
                    // Skip known problematic keys
                    if (key.Contains("seeded") || key.Contains("categories") || key.Contains("likes"))
                    {
                        continue;
                    }
                    
                    var tip = await _redis.GetAsync<Tip>(key);
                    if (tip != null && !tip.IsAiGenerated && !string.IsNullOrEmpty(tip.CreatedBy))
                    {
                        tipsContributors.Add(new UnifiedParticipant(
                            Name: tip.CreatedBy,
                            IpHash: tip.CreatedByIpHash ?? "unknown",
                            Score: tip.Likes,
                            Activity: $"Created tip: {tip.Title}",
                            LastActive: tip.CreatedAt
                        ));
                    }
                }
                catch (System.Text.Json.JsonException jsonEx)
                {
                    _logger.LogWarning(jsonEx, "JSON deserialization error for tip key {Key}, possibly corrupted data. Skipping this tip.", key);
                    // Continue processing other tips
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing tip {Key}", key);
                }
            }
            
            // Group by creator and sum their likes, then apply pagination
            var groupedContributors = tipsContributors
                .GroupBy(c => new { c.Name, c.IpHash })
                .Select(g => new UnifiedParticipant(
                    Name: g.Key.Name,
                    IpHash: g.Key.IpHash,
                    Score: g.Sum(c => c.Score),
                    Activity: $"Created {g.Count()} tip(s)",
                    LastActive: g.Max(c => c.LastActive)
                ))
                .OrderByDescending(c => c.Score)
                .ThenByDescending(c => c.LastActive)
                .Skip(offset)
                .Take(limit)
                .ToList();
            
            _logger.LogInformation("Found {Total} tips contributors, returning {Count} with pagination", tipsContributors.Count, groupedContributors.Count);
            return groupedContributors;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting tips contributors");
            return new List<UnifiedParticipant>();
        }
    }

    private async Task<string?> GetDisplayIpAsync(string sessionId)
    {
        try
        {
            return await _redis.GetStringAsync($"session:ip:display:{sessionId}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting display IP for session {SessionId}", sessionId);
            return null;
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

    public async Task<int> GetQuizParticipantsTotalCountAsync()
    {
        try
        {
            // Get the sorted attempts list and return its count
            var sortedAttempts = await GetSortedQuizAttemptsAsync();
            return sortedAttempts.Count;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting quiz participants total count");
            return 0;
        }
    }

    public async Task<int> GetGameParticipantsTotalCountAsync()
    {
        try
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var gameParticipants = new List<UnifiedParticipant>();
            
            // Get game participants from leaderboard
            var gameLeaderboard = await _redis.SortedSetRangeWithScoresAsync("game:leaderboard:alltime", 0, -1, true);
            
            foreach (var entry in gameLeaderboard)
            {
                try
                {
                    var playerData = entry.Member.Split(':');
                    if (playerData.Length >= 2)
                    {
                        var scoreId = playerData[1];
                        var gameScore = await _redis.GetAsync<GameScore>($"game:score:{scoreId}");
                        if (gameScore != null && gameScore.AchievedAt >= thirtyDaysAgo)
                        {
                            gameParticipants.Add(new UnifiedParticipant("", "", 0, "", gameScore.AchievedAt));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing game entry {Entry} for count", entry.Member);
                }
            }
            
            return gameParticipants.Count;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting game participants total count");
            return 0;
        }
    }

    public async Task<int> GetTipsContributorsTotalCountAsync()
    {
        try
        {
            var tipsContributors = new List<UnifiedParticipant>();
            
            // Get all tips from Redis
            var tipKeys = await _redis.GetKeysAsync("tips:*");
            var tipDataKeys = tipKeys.Where(k => !k.Contains("category:") && !k.Contains("likes:") && !k.Contains("categories")).ToList();
            
            foreach (var key in tipDataKeys)
            {
                try
                {
                    if (key.Contains("seeded") || key.Contains("categories") || key.Contains("likes"))
                    {
                        continue;
                    }
                    
                    var tip = await _redis.GetAsync<Tip>(key);
                    if (tip != null && !tip.IsAiGenerated && !string.IsNullOrEmpty(tip.CreatedBy))
                    {
                        tipsContributors.Add(new UnifiedParticipant(tip.CreatedBy, "", 0, "", tip.CreatedAt));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing tip {Key} for count", key);
                }
            }
            
            // Group by creator to get unique count
            var uniqueContributors = tipsContributors
                .GroupBy(c => c.Name)
                .Count();
            
            return uniqueContributors;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting tips contributors total count");
            return 0;
        }
    }
}