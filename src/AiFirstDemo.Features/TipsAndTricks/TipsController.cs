using Microsoft.AspNetCore.Mvc;
using AiFirstDemo.Features.TipsAndTricks.Models;
using Microsoft.Extensions.Logging;

namespace AiFirstDemo.Features.TipsAndTricks;

[ApiController]
[Route("api/[controller]")]
public class TipsController : ControllerBase
{
    private readonly ITipsService _tipsService;
    private readonly ILogger<TipsController> _logger;

    public TipsController(ITipsService tipsService, ILogger<TipsController> logger)
    {
        _tipsService = tipsService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<TipsCollectionResponse>> GetTips(
        [FromQuery] string? category = null,
        [FromQuery] string? sessionId = null)
    {
        try
        {
            var tips = await _tipsService.GetTipsAsync(category, sessionId);
            return Ok(tips);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tips for category {Category}", category);
            return StatusCode(500, "Error retrieving tips");
        }
    }

    [HttpGet("{tipId}")]
    public async Task<ActionResult<Tip>> GetTip(string tipId)
    {
        try
        {
            var tip = await _tipsService.GetTipAsync(tipId);
            if (tip == null)
            {
                return NotFound("Tip not found");
            }
            return Ok(tip);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tip {TipId}", tipId);
            return StatusCode(500, "Error retrieving tip");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Tip>> CreateTip([FromBody] CreateTipRequest request, [FromQuery] string sessionId)
    {
        try
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest("Session ID is required");
            }

            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest("Title and content are required");
            }

            var tip = await _tipsService.CreateTipAsync(request, sessionId);
            return CreatedAtAction(nameof(GetTip), new { tipId = tip.Id }, tip);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid tip creation request from session {SessionId}", sessionId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tip for session {SessionId}", sessionId);
            return StatusCode(500, "Error creating tip");
        }
    }

    [HttpPost("generate")]
    public async Task<ActionResult<List<Tip>>> GenerateTips([FromBody] GenerateTipsRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Category))
            {
                return BadRequest("Category is required");
            }

            if (request.Count < 1 || request.Count > 10)
            {
                return BadRequest("Count must be between 1 and 10");
            }

            var tips = await _tipsService.GenerateAiTipsAsync(request);
            return Ok(tips);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI tips for category {Category}", request.Category);
            return StatusCode(500, "Error generating tips");
        }
    }

    [HttpPost("{tipId}/like")]
    public async Task<ActionResult> LikeTip(string tipId, [FromQuery] string sessionId)
    {
        try
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest("Session ID is required");
            }

            var success = await _tipsService.LikeTipAsync(tipId, sessionId);
            if (!success)
            {
                return BadRequest("Could not like tip");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error liking tip {TipId} for session {SessionId}", tipId, sessionId);
            return StatusCode(500, "Error liking tip");
        }
    }

    [HttpDelete("{tipId}/like")]
    public async Task<ActionResult> UnlikeTip(string tipId, [FromQuery] string sessionId)
    {
        try
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest("Session ID is required");
            }

            var success = await _tipsService.UnlikeTipAsync(tipId, sessionId);
            if (!success)
            {
                return BadRequest("Could not unlike tip");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unliking tip {TipId} for session {SessionId}", tipId, sessionId);
            return StatusCode(500, "Error unliking tip");
        }
    }

    [HttpGet("categories")]
    public async Task<ActionResult<List<string>>> GetCategories()
    {
        try
        {
            var categories = await _tipsService.GetCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tip categories");
            return StatusCode(500, "Error retrieving categories");
        }
    }
}