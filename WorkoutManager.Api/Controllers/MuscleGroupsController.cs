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
    public async Task<ActionResult<IEnumerable<MuscleGroupDto>>> GetMuscleGroups()
    {
        try
        {
            var result = await _muscleGroupService.GetAllMuscleGroupsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<MuscleGroupDto>> GetMuscleGroupById(long id)
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
