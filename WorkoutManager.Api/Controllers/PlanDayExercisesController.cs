using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WorkoutManager.Api.Services;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Exceptions;
using WorkoutManager.BusinessLogic.Services.Interfaces;

namespace WorkoutManager.Api.Controllers;

[ApiController]
[Route("api/workout-plans/{planId}/training-days/{dayId}/exercises")]
public class PlanDayExercisesController : ControllerBase
{
    private readonly IPlanExerciseService _planExerciseService;
    private readonly IUserContextService _userContext;

    public PlanDayExercisesController(IPlanExerciseService planExerciseService, IUserContextService userContext)
    {
        _planExerciseService = planExerciseService;
        _userContext = userContext;
    }

    [HttpPost]
    public async Task<ActionResult<CreatedPlanDayExerciseDto>> AddExerciseToTrainingDay(
        int planId, 
        int dayId, 
        [FromBody] AddExerciseToTrainingDayCommand command)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            var result = await _planExerciseService.AddExerciseToDayAsync(planId, dayId, command, userId);
            return CreatedAtAction(null, new { id = result.Id }, result);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (BusinessRuleViolationException ex)
        {
            return StatusCode(403, new { error = ex.Message });
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

    [HttpDelete("{planDayExerciseId}")]
    public async Task<IActionResult> RemoveExerciseFromTrainingDay(int planId, int dayId, int planDayExerciseId)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            await _planExerciseService.RemoveExerciseFromDayAsync(planId, dayId, planDayExerciseId, userId);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (BusinessRuleViolationException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
