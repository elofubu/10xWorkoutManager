using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutManager.Api.Services;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Exceptions;
using WorkoutManager.BusinessLogic.Services.Interfaces;

namespace WorkoutManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IUserContextService _userContext;
    private readonly ILogger<SessionsController> _logger;

    public SessionsController(ISessionService sessionService, 
        IUserContextService userContext,
        ILogger<SessionsController> logger)
    {
        _sessionService = sessionService;
        _userContext = userContext;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<SessionSummaryDto>>> GetSessions([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            var result = await _sessionService.GetSessionHistoryAsync(userId, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something unexpeced happened geting all sessions.");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<SessionDetailsDto>> GetSessionById(long id)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            var session = await _sessionService.GetSessionByIdAsync(id, userId);
            return Ok(session);
        }
        catch (NotFoundException)
        {
            _logger.LogError("Get sesttion by ID returns not found exception");
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something unexpeced happened getting session by ID.");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<SessionDetailsDto>> GetActiveSession()
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            var session = await _sessionService.GetActiveSessionAsync(userId);
            if (session == null)
            {
                return NoContent();
            }
            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something unexpeced happened getting active session.");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<SessionDetailsDto>> StartSession([FromBody] StartSessionCommand command)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            var session = await _sessionService.StartSessionAsync(command.TrainingDayId, userId);
            return CreatedAtAction(nameof(GetSessionById), new { id = session.Id }, session);
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, "Start session returns not found exception");
            return NotFound(new { error = ex.Message });
        }
        catch (BusinessRuleViolationException ex)
        {
            _logger.LogError(ex, "Start session request returns bussines rule validation exception");
            return Conflict(new { error = ex.Message });
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Start session returns bad request exception");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something unexpeced happened when session starts.");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateSession(long id, [FromBody] UpdateSessionCommand command)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();

            if (command.EndTime.HasValue)
            {
                await _sessionService.FinishSessionAsync(id, command.Notes, userId);
            }
            else
            {
                await _sessionService.UpdateSessionNotesAsync(id, command.Notes, userId);
            }

            return NoContent();
        }
        catch (NotFoundException)
        {
            _logger.LogError($"Updating session returns not found exception. {command}");
            return NotFound();
        }
        catch (BusinessRuleViolationException ex)
        {
            _logger.LogError($"Updating session returns bussines rule valuation exception. {command}");
            return Conflict(new { error = ex.Message });
        }
        catch(BusinessLogic.Exceptions.UnauthorizedAccessException ex)
        {
            _logger.LogError($"Updating session returns unaauthorized exception. {command}");
            return Unauthorized(ex.Message);
        }
        catch (ValidationException ex)
        {
            _logger.LogError($"Updating session returns bad reqeust exception. {command}");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Something unexpeced happened when updating the session. {command}");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
