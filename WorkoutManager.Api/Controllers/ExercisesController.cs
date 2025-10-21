using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutManager.Api.Services;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Exceptions;
using WorkoutManager.BusinessLogic.Services.Interfaces;

namespace WorkoutManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ExercisesController : ControllerBase
{
    private readonly IExerciseService _exerciseService;
    private readonly IUserContextService _userContext;

    public ExercisesController(IExerciseService exerciseService, IUserContextService userContext)
    {
        _exerciseService = exerciseService;
        _userContext = userContext;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<ExerciseDto>>> GetExercises(
        [FromQuery] string? search,
        [FromQuery] long? muscleGroupId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            var result = await _exerciseService.GetExercisesAsync(userId, search, muscleGroupId, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ExerciseDto>> GetExerciseById(long id)
    {
        try
        {
            var exercise = await _exerciseService.GetExerciseByIdAsync(id);
            if (exercise == null) return NotFound();
            return Ok(exercise);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ExerciseDto>> CreateExercise([FromBody] CreateExerciseDto dto)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            var exercise = await _exerciseService.CreateExerciseAsync(dto, userId);
            return CreatedAtAction(nameof(GetExerciseById), new { id = exercise.Id }, exercise);
        }
        catch (BusinessRuleViolationException ex)
        {
            return Conflict(new { error = ex.Message });
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

    [HttpGet("{exerciseId:long}/previous-session")]
    public async Task<ActionResult<PreviousExercisePerformanceDto>> GetPreviousPerformance(long exerciseId)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            var performance = await _exerciseService.GetLastPerformanceAsync(exerciseId, userId);
            
            if (performance == null) return NotFound(new { error = "No previous performance found" });
            
            return Ok(performance);
        }
        catch (Exception ex)
        {   
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
