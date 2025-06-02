using Microsoft.AspNetCore.Mvc;
using AiFirstDemo.Features.Quiz.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AiFirstDemo.Features.Quiz;

[ApiController]
[Route("api/[controller]")]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;
    private readonly ILogger<QuizController> _logger;

    public QuizController(IQuizService quizService, ILogger<QuizController> logger)
    {
        _quizService = quizService;
        _logger = logger;
    }

    [HttpGet("questions")]
    public async Task<ActionResult<List<QuizQuestion>>> GetQuestions()
    {
        try
        {
            var questions = await _quizService.GetQuizQuestionsAsync();
            return Ok(questions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quiz questions");
            return StatusCode(500, "Error retrieving quiz questions");
        }
    }

    [HttpPost("submit")]
    public async Task<ActionResult<QuizResultResponse>> SubmitQuiz([FromBody] QuizSubmissionRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.SessionId))
            {
                return BadRequest("Session ID is required");
            }

            if (request.Answers == null || !request.Answers.Any())
            {
                return BadRequest("At least one answer is required");
            }

            var result = await _quizService.SubmitQuizAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid quiz submission for session {SessionId}", request.SessionId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting quiz for session {SessionId}", request.SessionId);
            return StatusCode(500, "Error processing quiz submission");
        }
    }

    [HttpGet("attempt/{sessionId}")]
    public async Task<ActionResult<QuizAttempt>> GetQuizAttempt(string sessionId)
    {
        try
        {
            var attempt = await _quizService.GetQuizAttemptAsync(sessionId);
            if (attempt == null)
            {
                return NotFound("Quiz attempt not found");
            }
            return Ok(attempt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quiz attempt for session {SessionId}", sessionId);
            return StatusCode(500, "Error retrieving quiz attempt");
        }
    }

    [HttpPost("generate")]
    public async Task<ActionResult<List<QuizQuestion>>> GenerateQuestions([FromQuery] int count = 10)
    {
        try
        {
            if (count < 1 || count > 20)
            {
                return BadRequest("Count must be between 1 and 20");
            }

            var questions = await _quizService.GenerateRandomQuestionsAsync(count);
            return Ok(questions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating quiz questions");
            return StatusCode(500, "Error generating questions");
        }
    }
}