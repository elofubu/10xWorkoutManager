using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WorkoutManager.Api.Services;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Exceptions;
using WorkoutManager.BusinessLogic.Services.Interfaces;

namespace WorkoutManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkoutPlansController : ControllerBase
{
    private readonly IWorkoutPlanService _workoutPlanService;
    private readonly IUserContextService _userContext;

    public WorkoutPlansController(IWorkoutPlanService workoutPlanService, IUserContextService userContext)
    {
        _workoutPlanService = workoutPlanService;
        _userContext = userContext;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<WorkoutPlanDto>>> GetWorkoutPlans([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            var result = await _workoutPlanService.GetWorkoutPlansAsync(userId, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkoutPlanDetailDto>> GetWorkoutPlanById(int id)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            var plan = await _workoutPlanService.GetWorkoutPlanByIdAsync(id, userId);
            return Ok(plan);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<CreatedWorkoutPlanDto>> CreateWorkoutPlan([FromBody] CreateWorkoutPlanCommand command)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            var result = await _workoutPlanService.CreateWorkoutPlanAsync(command, userId);
            return CreatedAtAction(nameof(GetWorkoutPlanById), new { id = result.Id }, result);
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

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWorkoutPlan(int id, [FromBody] UpdateWorkoutPlanPayload payload)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            await _workoutPlanService.UpdateWorkoutPlanAsync(id, payload, userId);
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
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkoutPlan(int id)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            await _workoutPlanService.DeleteWorkoutPlanAsync(id, userId);
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
