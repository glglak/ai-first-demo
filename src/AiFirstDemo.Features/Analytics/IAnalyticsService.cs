using AiFirstDemo.Features.Analytics.Models;

namespace AiFirstDemo.Features.Analytics;

public interface IAnalyticsService
{
    Task<DashboardData> GetDashboardDataAsync();
    Task<List<UserActivity>> GetActiveUsersAsync();
    Task TrackSessionActivityAsync(string sessionId, string activity);
    Task TrackQuizCompletionAsync(string sessionId, int score);
    Task TrackGamePlayAsync(string sessionId, int score);
    Task<List<HourlyActivity>> GetHourlyActivityAsync(DateTime date);
}