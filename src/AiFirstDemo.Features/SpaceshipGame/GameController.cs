using Microsoft.AspNetCore.Mvc;
using AiFirstDemo.Features.SpaceshipGame.Models;
using Microsoft.Extensions.Logging;

namespace AiFirstDemo.Features.SpaceshipGame;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly ILogger<GameController> _logger;

    public GameController(IGameService gameService, ILogger<GameController> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    [HttpPost("score")]
    public async Task<ActionResult<GameScore>> SubmitScore([FromBody] SubmitScoreRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.SessionId))
            {
                return BadRequest("Session ID is required");
            }

            if (string.IsNullOrEmpty(request.PlayerName))
            {
                return BadRequest("Player name is required");
            }

            if (request.Score < 0)
            {
                return BadRequest("Score must be non-negative");
            }

            var gameScore = await _gameService.SubmitScoreAsync(request);
            return Ok(gameScore);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid score submission from session {SessionId}", request.SessionId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting score for session {SessionId}", request.SessionId);
            return StatusCode(500, "Error submitting score");
        }
    }

    [HttpGet("leaderboard")]
    public async Task<ActionResult<LeaderboardResponse>> GetLeaderboard([FromQuery] string? sessionId = null)
    {
        try
        {
            var leaderboard = await _gameService.GetLeaderboardAsync(sessionId);
            return Ok(leaderboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leaderboard");
            return StatusCode(500, "Error retrieving leaderboard");
        }
    }

    [HttpGet("scores/{sessionId}")]
    public async Task<ActionResult<List<GameScore>>> GetPlayerScores(string sessionId)
    {
        try
        {
            var scores = await _gameService.GetPlayerScoresAsync(sessionId);
            return Ok(scores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting player scores for session {SessionId}", sessionId);
            return StatusCode(500, "Error retrieving player scores");
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<GameStats>> GetGameStats()
    {
        try
        {
            var stats = await _gameService.GetGameStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting game stats");
            return StatusCode(500, "Error retrieving game statistics");
        }
    }
}