using Microsoft.AspNetCore.Mvc;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Services.Interfaces;

namespace WorkoutManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MuscleGroupsController : ControllerBase
{
    private readonly IMuscleGroupService _muscleGroupService;

    public MuscleGroupsController(IMuscleGroupService muscleGroupService)
    {
        _muscleGroupService = muscleGroupService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<MuscleGroupDto>>> GetMuscleGroups([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await _muscleGroupService.GetAllMuscleGroupsAsync(page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MuscleGroupDto>> GetMuscleGroupById(int id)
    {
        try
        {
            var muscleGroup = await _muscleGroupService.GetMuscleGroupByIdAsync(id);
            if (muscleGroup == null) return NotFound();
            return Ok(muscleGroup);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
