using Microsoft.AspNetCore.Mvc;
using AiFirstDemo.Features.Shared.Models;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AiFirstDemo.Features.UserSessions;

[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly IUserSessionService _sessionService;
    private readonly ILogger<SessionsController> _logger;

    public SessionsController(IUserSessionService sessionService, ILogger<SessionsController> logger)
    {
        _sessionService = sessionService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<UserSessionResponse>> CreateSession([FromBody] CreateSessionNameRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Name is required");
            }

            // Get client IP address
            var clientIp = GetClientIpAddress();
            
            // Always allow session creation, but try to get quiz info with fallbacks
            var createRequest = new CreateUserSessionRequest(request.Name, clientIp);
            var session = await _sessionService.CreateSessionAsync(createRequest);

            var response = new UserSessionResponse(
                SessionId: session.SessionId,
                Name: session.Name,
                CreatedAt: session.CreatedAt,
                HasCompletedQuiz: session.HasCompletedQuiz
            );

            // Try to get quiz attempt info, but don't fail session creation if Redis is down
            try
            {
                var quizAttempts = await _sessionService.GetQuizAttemptsForIpAsync(clientIp);
                var canTakeQuiz = await _sessionService.CanTakeQuizAsync(clientIp);
                
                // Add quiz attempt info to response headers for frontend to handle
                Response.Headers.Add("X-Quiz-Attempts", quizAttempts.ToString());
                Response.Headers.Add("X-Can-Take-Quiz", canTakeQuiz.ToString().ToLower());
                Response.Headers.Add("X-Max-Quiz-Attempts", "3");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not retrieve quiz attempt info for IP {ClientIp}, using defaults", clientIp);
                
                // Set safe defaults if Redis is unavailable
                Response.Headers.Add("X-Quiz-Attempts", "0");
                Response.Headers.Add("X-Can-Take-Quiz", "true");
                Response.Headers.Add("X-Max-Quiz-Attempts", "3");
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for user {Name}", request.Name);
            return StatusCode(500, "Error creating session");
        }
    }

    [HttpGet("{sessionId}")]
    public async Task<ActionResult<UserSessionResponse>> GetSession(string sessionId)
    {
        try
        {
            var session = await _sessionService.GetSessionAsync(sessionId);
            if (session == null)
            {
                return NotFound("Session not found");
            }

            var response = new UserSessionResponse(
                SessionId: session.SessionId,
                Name: session.Name,
                CreatedAt: session.CreatedAt,
                HasCompletedQuiz: session.HasCompletedQuiz
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session {SessionId}", sessionId);
            return StatusCode(500, "Error retrieving session");
        }
    }

    [HttpPost("{sessionId}/activity")]
    public async Task<ActionResult> UpdateActivity(string sessionId)
    {
        try
        {
            var success = await _sessionService.UpdateLastActivityAsync(sessionId);
            if (!success)
            {
                return NotFound("Session not found");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating activity for session {SessionId}", sessionId);
            return StatusCode(500, "Error updating activity");
        }
    }

    [HttpPost("cleanup-quiz-counters")]
    public async Task<ActionResult<object>> CleanupQuizCounters()
    {
        try
        {
            var cleanedCount = await _sessionService.CleanupCorruptedQuizCountersAsync();
            
            _logger.LogInformation("Quiz counter cleanup completed. Cleaned {Count} corrupted keys", cleanedCount);
            
            return Ok(new { 
                message = "Quiz counter cleanup completed", 
                cleanedKeys = cleanedCount,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during quiz counter cleanup");
            return StatusCode(500, "Error during cleanup");
        }
    }

    [HttpGet("health")]
    public async Task<ActionResult<object>> HealthCheck()
    {
        try
        {
            var startTime = DateTime.UtcNow;
            
            // Test basic Redis connectivity
            var testKey = $"health:test:{Guid.NewGuid()}";
            var testValue = "test-value";
            
            // Try to set and get a test value
            var canTakeQuiz = await _sessionService.CanTakeQuizAsync("127.0.0.1");
            var elapsed = DateTime.UtcNow - startTime;
            
            return Ok(new
            {
                status = "healthy",
                redis = new
                {
                    connected = true,
                    responseTime = $"{elapsed.TotalMilliseconds:F0}ms",
                    canPerformOperations = true
                },
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check failed");
            
            return Ok(new
            {
                status = "degraded",
                redis = new
                {
                    connected = false,
                    error = ex.Message,
                    canPerformOperations = false
                },
                timestamp = DateTime.UtcNow
            });
        }
    }

    private string GetClientIpAddress()
    {
        // Check for forwarded IP first (common in load balancers/proxies)
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        // Check for real IP header
        var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to connection remote IP
        return Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    }
}