namespace AiFirstDemo.Features.SpaceshipGame.Models;

public record SubmitScoreRequest(
    string SessionId,
    string PlayerName,
    int Score,
    TimeSpan GameDuration,
    int Level,
    int AsteroidsDestroyed
);

public record GameScore(
    string SessionId,
    string PlayerName,
    int Score,
    DateTime AchievedAt,
    TimeSpan GameDuration,
    int Level,
    int AsteroidsDestroyed
);

public record LeaderboardResponse(
    List<LeaderboardEntry> DailyLeaderboard,
    List<LeaderboardEntry> WeeklyLeaderboard,
    List<LeaderboardEntry> AllTimeLeaderboard,
    LeaderboardEntry? PlayerBest
);

public record LeaderboardEntry(
    string PlayerName,
    int Score,
    DateTime AchievedAt,
    int Level,
    int Rank
);

public record GameStats(
    int TotalGamesPlayed,
    int TotalPlayersToday,
    double AverageScore,
    int HighestScoreToday,
    string TopPlayerToday
);

public interface IGameService
{
    Task<GameScore> SubmitScoreAsync(SubmitScoreRequest request);
    Task<LeaderboardResponse> GetLeaderboardAsync(string? sessionId = null);
    Task<List<GameScore>> GetPlayerScoresAsync(string sessionId);
    Task<GameStats> GetGameStatsAsync();
    Task<bool> ValidateScoreAsync(SubmitScoreRequest request);
} 