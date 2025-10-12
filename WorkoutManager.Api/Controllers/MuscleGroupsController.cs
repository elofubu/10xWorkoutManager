using Microsoft.AspNetCore.Mvc;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MuscleGroupsController : ControllerBase
{
    [HttpGet]
    public ActionResult<PaginatedList<MuscleGroupDto>> GetMuscleGroups([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        // This is a mock implementation. In the future, this will be replaced with a call to the database.
        var muscleGroups = new List<MuscleGroupDto>
        {
            new() { Id = 1, Name = "Chest" },
            new() { Id = 2, Name = "Back" },
            new() { Id = 3, Name = "Shoulders" },
            new() { Id = 4, Name = "Biceps" },
            new() { Id = 5, Name = "Triceps" },
            new() { Id = 6, Name = "Legs" },
            new() { Id = 7, Name = "Abs" }
        };

        var totalCount = muscleGroups.Count;
        var paginatedData = muscleGroups.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var result = new PaginatedList<MuscleGroupDto>
        {
            Data = paginatedData,
            Pagination = new PaginationInfo
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            }
        };

        return Ok(result);
    }
}
