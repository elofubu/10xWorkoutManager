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
[Route("api/sessions/{sessionId:long}/exercises")]
public class SessionExercisesController : ControllerBase
{
    private readonly ISessionExerciseService _sessionExerciseService;
    private readonly IUserContextService _userContext;

    public SessionExercisesController(ISessionExerciseService sessionExerciseService, IUserContextService userContext)
    {
        _sessionExerciseService = sessionExerciseService;
        _userContext = userContext;
    }

    [HttpPut("{sessionExerciseId:long}")]
    public async Task<ActionResult<SessionExerciseDetailsDto>> UpdateSessionExercise(
        long sessionId,
        long sessionExerciseId,
        [FromBody] UpdateSessionExerciseCommand command)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            var result = await _sessionExerciseService.UpdateSessionExerciseAsync(sessionId, sessionExerciseId, command, userId);
            return Ok(result);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
