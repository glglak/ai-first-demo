namespace AiFirstDemo.Features.Analytics.Models;

public record DashboardData(
    int TotalSessions,
    int ActiveSessionsNow,
    int QuizzesCompleted,
    int GamesPlayed,
    double AverageQuizScore,
    int TopGameScore,
    List<HourlyActivity> HourlyActivity,
    List<CategoryPerformance> QuizPerformance,
    List<PopularTip> PopularTips
);

public record HourlyActivity(
    DateTime Hour,
    int Sessions,
    int QuizAttempts,
    int GamePlays
);

public record CategoryPerformance(
    string Category,
    double AverageScore,
    int TotalAttempts
);

public record PopularTip(
    string Title,
    string Category,
    int Likes,
    string CreatedBy
);

public record UserActivity(
    string SessionId,
    string UserName,
    DateTime LastActivity,
    bool CompletedQuiz,
    int GameAttempts,
    int BestScore
);

public record UnifiedLeaderboardResponse(
    List<UnifiedParticipant> QuizParticipants,
    List<UnifiedParticipant> GameParticipants,
    List<UnifiedParticipant> TipsContributors
);

public record UnifiedParticipant(
    string Name,
    string IpHash,
    int Score,
    string Activity,
    DateTime LastActive
);

public record PaginatedResponse<T>(
    List<T> Data,
    int Total,
    int Page,
    int PageSize,
    bool HasNext,
    bool HasPrevious
);