using AiFirstDemo.Features.SpaceshipGame.Models;
using AiFirstDemo.Infrastructure.Redis;
using AiFirstDemo.Features.UserSessions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using AiFirstDemo.Features.Shared.Hubs;

namespace AiFirstDemo.Features.SpaceshipGame;

public class GameService : IGameService
{
    private readonly IRedisService _redis;
    private readonly IUserSessionService _userSession;
    private readonly IHubContext<GameHub> _gameHub;
    private readonly ILogger<GameService> _logger;
    
    private const string GAME_SCORE_PREFIX = "game:score:";
    private const string DAILY_LEADERBOARD = "game:leaderboard:daily:";
    private const string WEEKLY_LEADERBOARD = "game:leaderboard:weekly:";
    private const string ALLTIME_LEADERBOARD = "game:leaderboard:alltime";
    private const string PLAYER_SCORES_PREFIX = "game:player:";
    private const string GAME_STATS = "game:stats:";

    public GameService(
        IRedisService redis,
        IUserSessionService userSession,
        IHubContext<GameHub> gameHub,
        ILogger<GameService> logger)
    {
        _redis = redis;
        _userSession = userSession;
        _gameHub = gameHub;
        _logger = logger;
    }

    public async Task<GameScore> SubmitScoreAsync(SubmitScoreRequest request)
    {
        // Validate session
        var session = await _userSession.GetSessionAsync(request.SessionId);
        if (session == null)
        {
            throw new InvalidOperationException("Invalid session");
        }

        // Basic score validation
        if (!await ValidateScoreAsync(request))
        {
            throw new InvalidOperationException("Invalid score detected");
        }

        var gameScore = new GameScore(
            SessionId: request.SessionId,
            PlayerName: request.PlayerName,
            Score: request.Score,
            AchievedAt: DateTime.UtcNow,
            GameDuration: request.GameDuration,
            Level: request.Level,
            AsteroidsDestroyed: request.AsteroidsDestroyed
        );

        var scoreId = Guid.NewGuid().ToString();
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var week = GetWeekKey(DateTime.UtcNow);

        // Store individual score
        await _redis.SetAsync($"{GAME_SCORE_PREFIX}{scoreId}", gameScore, TimeSpan.FromDays(30));

        // Add to player's scores
        await _redis.ListPushAsync($"{PLAYER_SCORES_PREFIX}{request.SessionId}", scoreId);

        // Add to leaderboards
        await _redis.SortedSetAddAsync($"{DAILY_LEADERBOARD}{today}", 
            $"{request.PlayerName}:{scoreId}", request.Score);
        await _redis.SortedSetAddAsync($"{WEEKLY_LEADERBOARD}{week}", 
            $"{request.PlayerName}:{scoreId}", request.Score);
        await _redis.SortedSetAddAsync(ALLTIME_LEADERBOARD, 
            $"{request.PlayerName}:{scoreId}", request.Score);

        // Update stats
        await UpdateGameStatsAsync(request);

        // Notify connected clients
        await _gameHub.Clients.All.SendAsync("ScoreUpdate", new {
            playerName = request.PlayerName,
            score = request.Score,
            timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("Score submitted: {PlayerName} scored {Score}", 
            request.PlayerName, request.Score);

        return gameScore;
    }

    public async Task<LeaderboardResponse> GetLeaderboardAsync(string? sessionId = null)
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var week = GetWeekKey(DateTime.UtcNow);

        var dailyScores = await _redis.SortedSetRangeWithScoresAsync(
            $"{DAILY_LEADERBOARD}{today}", 0, 9, true);
        var weeklyScores = await _redis.SortedSetRangeWithScoresAsync(
            $"{WEEKLY_LEADERBOARD}{week}", 0, 9, true);
        var allTimeScores = await _redis.SortedSetRangeWithScoresAsync(
            ALLTIME_LEADERBOARD, 0, 9, true);

        var dailyLeaderboard = await BuildLeaderboardEntries(dailyScores);
        var weeklyLeaderboard = await BuildLeaderboardEntries(weeklyScores);
        var allTimeLeaderboard = await BuildLeaderboardEntries(allTimeScores);

        LeaderboardEntry? playerBest = null;
        if (!string.IsNullOrEmpty(sessionId))
        {
            playerBest = await GetPlayerBestScore(sessionId);
        }

        return new LeaderboardResponse(
            DailyLeaderboard: dailyLeaderboard,
            WeeklyLeaderboard: weeklyLeaderboard,
            AllTimeLeaderboard: allTimeLeaderboard,
            PlayerBest: playerBest
        );
    }

    public async Task<List<GameScore>> GetPlayerScoresAsync(string sessionId)
    {
        var scoreIds = await _redis.ListRangeAsync($"{PLAYER_SCORES_PREFIX}{sessionId}");
        var scores = new List<GameScore>();

        foreach (var scoreId in scoreIds)
        {
            var score = await _redis.GetAsync<GameScore>($"{GAME_SCORE_PREFIX}{scoreId}");
            if (score != null)
            {
                scores.Add(score);
            }
        }

        return scores.OrderByDescending(s => s.Score).ToList();
    }

    public async Task<GameStats> GetGameStatsAsync()
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var statsKey = $"{GAME_STATS}{today}";

        var stats = await _redis.HashGetAllAsync(statsKey);
        
        if (!stats.Any())
        {
            return new GameStats(
                TotalGamesPlayed: 0,
                TotalPlayersToday: 0,
                AverageScore: 0,
                HighestScoreToday: 0,
                TopPlayerToday: "N/A"
            );
        }

        return new GameStats(
            TotalGamesPlayed: int.Parse(stats.GetValueOrDefault("totalGames", "0")),
            TotalPlayersToday: int.Parse(stats.GetValueOrDefault("totalPlayers", "0")),
            AverageScore: double.Parse(stats.GetValueOrDefault("averageScore", "0")),
            HighestScoreToday: int.Parse(stats.GetValueOrDefault("highestScore", "0")),
            TopPlayerToday: stats.GetValueOrDefault("topPlayer", "N/A")
        );
    }

    public async Task<bool> ValidateScoreAsync(SubmitScoreRequest request)
    {
        // Basic validation rules for the spaceship game
        // These would be more sophisticated in a real game
        
        // Score should be reasonable for game duration
        var maxScorePerSecond = 100; // Arbitrary limit
        var maxPossibleScore = (int)(request.GameDuration.TotalSeconds * maxScorePerSecond);
        
        if (request.Score > maxPossibleScore)
        {
            _logger.LogWarning("Suspicious score: {Score} for duration {Duration}", 
                request.Score, request.GameDuration);
            return false;
        }

        // Level should correlate with score somewhat
        if (request.Level > 1 && request.Score < request.Level * 100)
        {
            _logger.LogWarning("Score {Score} too low for level {Level}", 
                request.Score, request.Level);
            return false;
        }

        // Game duration should be reasonable
        if (request.GameDuration.TotalMinutes > 30 || request.GameDuration.TotalSeconds < 10)
        {
            _logger.LogWarning("Suspicious game duration: {Duration}", request.GameDuration);
            return false;
        }

        return true;
    }

    private async Task<List<LeaderboardEntry>> BuildLeaderboardEntries(
        List<(string Member, double Score)> scores)
    {
        var entries = new List<LeaderboardEntry>();
        int rank = 1;

        foreach (var (member, score) in scores)
        {
            var parts = member.Split(':');
            if (parts.Length >= 2)
            {
                var playerName = parts[0];
                var scoreId = parts[1];
                
                var gameScore = await _redis.GetAsync<GameScore>($"{GAME_SCORE_PREFIX}{scoreId}");
                if (gameScore != null)
                {
                    entries.Add(new LeaderboardEntry(
                        PlayerName: playerName,
                        Score: (int)score,
                        AchievedAt: gameScore.AchievedAt,
                        Level: gameScore.Level,
                        Rank: rank++
                    ));
                }
            }
        }

        return entries;
    }

    private async Task<LeaderboardEntry?> GetPlayerBestScore(string sessionId)
    {
        var playerScores = await GetPlayerScoresAsync(sessionId);
        var bestScore = playerScores.OrderByDescending(s => s.Score).FirstOrDefault();
        
        if (bestScore == null) return null;

        // Find rank in all-time leaderboard
        var playerRank = await _redis.SortedSetRangeWithScoresAsync(
            ALLTIME_LEADERBOARD, 0, -1, true);
        
        var rank = playerRank.FindIndex(entry => 
            entry.Member.StartsWith(bestScore.PlayerName) && 
            (int)entry.Score == bestScore.Score) + 1;

        return new LeaderboardEntry(
            PlayerName: bestScore.PlayerName,
            Score: bestScore.Score,
            AchievedAt: bestScore.AchievedAt,
            Level: bestScore.Level,
            Rank: rank > 0 ? rank : 999
        );
    }

    private async Task UpdateGameStatsAsync(SubmitScoreRequest request)
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var statsKey = $"{GAME_STATS}{today}";

        // Increment total games
        await _redis.IncrementAsync($"{statsKey}:totalGames");
        
        // Update highest score if needed
        var currentHigh = await _redis.HashGetAsync(statsKey, "highestScore");
        var currentHighScore = int.Parse(currentHigh ?? "0");
        
        if (request.Score > currentHighScore)
        {
            await _redis.HashSetAsync(statsKey, "highestScore", request.Score.ToString());
            await _redis.HashSetAsync(statsKey, "topPlayer", request.PlayerName);
        }

        // Update running average (simplified)
        var totalGames = await _redis.HashGetAsync(statsKey, "totalGames");
        var currentAvg = await _redis.HashGetAsync(statsKey, "averageScore");
        
        if (int.TryParse(totalGames, out var games) && double.TryParse(currentAvg, out var avg))
        {
            var newAvg = ((avg * (games - 1)) + request.Score) / games;
            await _redis.HashSetAsync(statsKey, "averageScore", newAvg.ToString("F2"));
        }
        else
        {
            await _redis.HashSetAsync(statsKey, "averageScore", request.Score.ToString());
        }
    }

    private string GetWeekKey(DateTime date)
    {
        var startOfWeek = date.AddDays(-(int)date.DayOfWeek);
        return startOfWeek.ToString("yyyy-MM-dd");
    }
}