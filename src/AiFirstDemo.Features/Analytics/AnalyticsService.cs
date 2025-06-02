using AiFirstDemo.Features.Analytics.Models;
using AiFirstDemo.Infrastructure.Redis;
using Microsoft.Extensions.Logging;

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
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        
        // Get basic stats
        var totalSessions = await _redis.HashGetAsync($"{ANALYTICS_PREFIX}{DAILY_STATS}{today}", "totalSessions") ?? "0";
        var quizzesCompleted = await _redis.HashGetAsync($"{ANALYTICS_PREFIX}{DAILY_STATS}{today}", "quizzesCompleted") ?? "0";
        var gamesPlayed = await _redis.HashGetAsync($"{ANALYTICS_PREFIX}{DAILY_STATS}{today}", "gamesPlayed") ?? "0";
        
        // Get active sessions (simplified - last 30 minutes)
        var activeSessionsCount = 0; // Would implement real-time tracking
        
        // Get hourly activity for the last 24 hours
        var hourlyActivity = await GetHourlyActivityAsync(DateTime.UtcNow);
        
        return new DashboardData(
            TotalSessions: int.Parse(totalSessions),
            ActiveSessionsNow: activeSessionsCount,
            QuizzesCompleted: int.Parse(quizzesCompleted),
            GamesPlayed: int.Parse(gamesPlayed),
            AverageQuizScore: 0, // Would calculate from quiz data
            TopGameScore: 0, // Would get from game leaderboard
            HourlyActivity: hourlyActivity,
            QuizPerformance: new List<CategoryPerformance>(),
            PopularTips: new List<PopularTip>()
        );
    }

    public async Task<List<UserActivity>> GetActiveUsersAsync()
    {
        // Implementation would track active users
        return new List<UserActivity>();
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
        var activities = new List<HourlyActivity>();
        
        for (int i = 0; i < 24; i++)
        {
            var hour = date.AddHours(-i).ToString("yyyy-MM-dd-HH");
            var sessions = await _redis.HashGetAsync($"{ANALYTICS_PREFIX}{HOURLY_STATS}{hour}", "sessions") ?? "0";
            var quizAttempts = await _redis.HashGetAsync($"{ANALYTICS_PREFIX}{HOURLY_STATS}{hour}", "quizAttempts") ?? "0";
            var gamePlays = await _redis.HashGetAsync($"{ANALYTICS_PREFIX}{HOURLY_STATS}{hour}", "gamePlays") ?? "0";
            
            activities.Add(new HourlyActivity(
                Hour: date.AddHours(-i),
                Sessions: int.Parse(sessions),
                QuizAttempts: int.Parse(quizAttempts),
                GamePlays: int.Parse(gamePlays)
            ));
        }
        
        return activities.OrderBy(a => a.Hour).ToList();
    }
}