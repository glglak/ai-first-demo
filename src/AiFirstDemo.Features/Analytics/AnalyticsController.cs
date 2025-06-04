using Microsoft.AspNetCore.Mvc;
using AiFirstDemo.Features.Analytics.Models;
using Microsoft.Extensions.Logging;

namespace AiFirstDemo.Features.Analytics;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardData>> GetDashboard()
    {
        try
        {
            var dashboard = await _analyticsService.GetDashboardDataAsync();
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard data");
            return StatusCode(500, "Error retrieving dashboard data");
        }
    }

    [HttpGet("unified-leaderboard")]
    public async Task<ActionResult<UnifiedLeaderboardResponse>> GetUnifiedLeaderboard()
    {
        var leaderboard = await _analyticsService.GetUnifiedLeaderboardAsync();
        return Ok(leaderboard);
    }

    [HttpGet("leaderboard/quiz")]
    public async Task<ActionResult<PaginatedResponse<UnifiedParticipant>>> GetQuizParticipants([FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        try
        {
            var participants = await _analyticsService.GetQuizParticipantsAsync(limit, offset);
            var totalCount = await _analyticsService.GetQuizParticipantsTotalCountAsync();
            
            var currentPage = (offset / limit) + 1;
            var hasNext = offset + limit < totalCount;
            var hasPrevious = offset > 0;
            
            var response = new PaginatedResponse<UnifiedParticipant>(
                Data: participants,
                Total: totalCount,
                Page: currentPage,
                PageSize: limit,
                HasNext: hasNext,
                HasPrevious: hasPrevious
            );
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quiz participants");
            return StatusCode(500, "Error retrieving quiz participants");
        }
    }

    [HttpGet("leaderboard/game")]
    public async Task<ActionResult<PaginatedResponse<UnifiedParticipant>>> GetGameParticipants([FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        try
        {
            var participants = await _analyticsService.GetGameParticipantsAsync(limit, offset);
            var totalCount = await _analyticsService.GetGameParticipantsTotalCountAsync();
            
            var currentPage = (offset / limit) + 1;
            var hasNext = offset + limit < totalCount;
            var hasPrevious = offset > 0;
            
            var response = new PaginatedResponse<UnifiedParticipant>(
                Data: participants,
                Total: totalCount,
                Page: currentPage,
                PageSize: limit,
                HasNext: hasNext,
                HasPrevious: hasPrevious
            );
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting game participants");
            return StatusCode(500, "Error retrieving game participants");
        }
    }

    [HttpGet("leaderboard/tips")]
    public async Task<ActionResult<PaginatedResponse<UnifiedParticipant>>> GetTipsContributors([FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        try
        {
            var contributors = await _analyticsService.GetTipsContributorsAsync(limit, offset);
            var totalCount = await _analyticsService.GetTipsContributorsTotalCountAsync();
            
            var currentPage = (offset / limit) + 1;
            var hasNext = offset + limit < totalCount;
            var hasPrevious = offset > 0;
            
            var response = new PaginatedResponse<UnifiedParticipant>(
                Data: contributors,
                Total: totalCount,
                Page: currentPage,
                PageSize: limit,
                HasNext: hasNext,
                HasPrevious: hasPrevious
            );
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tips contributors");
            return StatusCode(500, "Error retrieving tips contributors");
        }
    }

    [HttpGet("users/active")]
    public async Task<ActionResult<List<UserActivity>>> GetActiveUsers()
    {
        try
        {
            var users = await _analyticsService.GetActiveUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active users");
            return StatusCode(500, "Error retrieving active users");
        }
    }

    [HttpPost("track/{sessionId}")]
    public async Task<ActionResult> TrackActivity(string sessionId, [FromBody] TrackActivityRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Activity))
            {
                return BadRequest("Activity is required");
            }

            await _analyticsService.TrackSessionActivityAsync(sessionId, request.Activity);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking activity for session {SessionId}", sessionId);
            return StatusCode(500, "Error tracking activity");
        }
    }

    [HttpGet("activity/{date}")]
    public async Task<ActionResult<List<HourlyActivity>>> GetHourlyActivity(DateTime date)
    {
        try
        {
            var activity = await _analyticsService.GetHourlyActivityAsync(date);
            return Ok(activity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hourly activity for {Date}", date);
            return StatusCode(500, "Error retrieving hourly activity");
        }
    }
}

public record TrackActivityRequest(string Activity);