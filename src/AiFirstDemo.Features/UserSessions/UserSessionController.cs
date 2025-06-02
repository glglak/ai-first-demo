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
    public async Task<ActionResult<UserSessionResponse>> CreateSession([FromBody] CreateUserSessionRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Name is required");
            }

            // Get client IP address
            var clientIp = GetClientIpAddress();
            
            // Check if session already exists for this IP
            if (await _sessionService.SessionExistsForIpAsync(clientIp))
            {
                return Conflict("A session already exists for this IP address");
            }

            var createRequest = new CreateUserSessionRequest(request.Name, clientIp);
            var session = await _sessionService.CreateSessionAsync(createRequest);

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