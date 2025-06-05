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
    Task<UnifiedLeaderboardResponse> GetUnifiedLeaderboardAsync();
    
    // Separate methods for each leaderboard section with pagination
    Task<List<UnifiedParticipant>> GetQuizParticipantsAsync(int limit = 10, int offset = 0);
    Task<List<UnifiedParticipant>> GetGameParticipantsAsync(int limit = 10, int offset = 0);
    Task<List<UnifiedParticipant>> GetTipsContributorsAsync(int limit = 10, int offset = 0);
    
    // Total count methods for pagination
    Task<int> GetQuizParticipantsTotalCountAsync();
    Task<int> GetGameParticipantsTotalCountAsync();
    Task<int> GetTipsContributorsTotalCountAsync();
    
    // Debug method for troubleshooting
    Task<object> GetRedisDebugInfoAsync();
}