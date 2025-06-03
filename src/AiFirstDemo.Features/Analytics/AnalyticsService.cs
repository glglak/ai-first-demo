using AiFirstDemo.Features.Analytics.Models;
using AiFirstDemo.Infrastructure.Redis;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using AiFirstDemo.Features.Quiz.Models;
using AiFirstDemo.Features.Shared.Models;
using AiFirstDemo.Features.TipsAndTricks.Models;
using AiFirstDemo.Features.SpaceshipGame.Models;

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
            var quizParticipants = new List<UnifiedParticipant>();
            
            _logger.LogInformation("Getting quiz participants from Redis with limit {Limit}, offset {Offset}...", limit, offset);
            
            // Get all quiz attempts from Redis
            // Quiz attempts are stored as "quiz:attempt:{sessionId}"
            var quizKeys = await _redis.GetKeysAsync("quiz:attempt:*");
            
            foreach (var key in quizKeys)
            {
                try
                {
                    var attempt = await _redis.GetAsync<QuizAttempt>(key);
                    if (attempt != null)
                    {
                        var sessionId = key.Replace("quiz:attempt:", "");
                        
                        // Get session data to get user name and IP hash
                        var session = await _redis.GetAsync<UserSession>($"user:session:{sessionId}");
                        if (session != null)
                        {
                            quizParticipants.Add(new UnifiedParticipant(
                                Name: session.Name,
                                IpHash: session.IpHash,
                                Score: attempt.Score,
                                Activity: "Quiz Completed",
                                LastActive: attempt.CompletedAt
                            ));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing quiz attempt {Key}", key);
                }
            }
            
            // Apply pagination: sort by score descending, then apply offset and limit
            var paginatedResults = quizParticipants
                .OrderByDescending(p => p.Score)
                .ThenByDescending(p => p.LastActive)
                .Skip(offset)
                .Take(limit)
                .ToList();
            
            _logger.LogInformation("Found {Total} quiz participants, returning {Count} with pagination", quizParticipants.Count, paginatedResults.Count);
            return paginatedResults;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting quiz participants");
            return new List<UnifiedParticipant>();
        }
    }

    public async Task<List<UnifiedParticipant>> GetGameParticipantsAsync(int limit = 10, int offset = 0)
    {
        try
        {
            var gameParticipants = new List<UnifiedParticipant>();
            
            _logger.LogInformation("Getting game participants from leaderboard with limit {Limit}, offset {Offset}...", limit, offset);
            
            // Get game participants from leaderboard - this is fast since it's a sorted set
            // We get more than needed to handle offset properly, but cap at reasonable limit
            var maxFetch = Math.Min(offset + limit, 100); // Don't fetch more than 100 total
            var gameLeaderboard = await _redis.SortedSetRangeWithScoresAsync("game:leaderboard:alltime", 0, maxFetch - 1, true);
            
            foreach (var entry in gameLeaderboard)
            {
                try
                {
                    var playerData = entry.Member.Split(':');
                    if (playerData.Length >= 2)
                    {
                        var playerName = playerData[0];
                        var scoreId = playerData[1];
                        
                        // Try to get more details from the score record
                        var gameScore = await _redis.GetAsync<GameScore>($"game:score:{scoreId}");
                        var ipHash = "game-player";
                        var lastActive = DateTime.UtcNow;
                        
                        if (gameScore != null)
                        {
                            lastActive = gameScore.AchievedAt;
                        }
                        
                        gameParticipants.Add(new UnifiedParticipant(
                            Name: playerName,
                            IpHash: ipHash,
                            Score: (int)entry.Score,
                            Activity: "Game High Score",
                            LastActive: lastActive
                        ));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing game entry {Entry}", entry.Member);
                }
            }
            
            // Apply pagination (data is already sorted by Redis)
            var paginatedResults = gameParticipants
                .Skip(offset)
                .Take(limit)
                .ToList();
            
            _logger.LogInformation("Found {Total} game participants, returning {Count} with pagination", gameParticipants.Count, paginatedResults.Count);
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
}