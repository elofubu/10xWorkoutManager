using Microsoft.AspNetCore.Mvc;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExercisesController : ControllerBase
{
    private static readonly List<ExerciseDto> _exercises = new()
    {
        new() { Id = 101, UserId = null, MuscleGroupId = 1, Name = "Bench Press" },
        new() { Id = 102, UserId = null, MuscleGroupId = 1, Name = "Incline Dumbbell Press" },
        new() { Id = 103, UserId = null, MuscleGroupId = 2, Name = "Pull Up" },
        new() { Id = 104, UserId = null, MuscleGroupId = 2, Name = "Bent Over Row" },
        new() { Id = 105, UserId = Guid.NewGuid(), MuscleGroupId = 5, Name = "Custom Bicep Curl" }
    };

    [HttpGet]
    public ActionResult<PaginatedList<ExerciseDto>> GetExercises(
        [FromQuery] string? search,
        [FromQuery] int? muscleGroupId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _exercises.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e => e.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (muscleGroupId.HasValue)
        {
            query = query.Where(e => e.MuscleGroupId == muscleGroupId.Value);
        }

        var totalCount = query.Count();
        var paginatedData = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var result = new PaginatedList<ExerciseDto>
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

    [HttpPost]
    public ActionResult<ExerciseDto> CreateExercise([FromBody] CreateExerciseCommand command)
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return BadRequest("Exercise name cannot be empty.");
        }

        // Mocking a conflict check
        if (_exercises.Any(e => e.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return Conflict("An exercise with this name already exists.");
        }

        var newExercise = new ExerciseDto
        {
            Id = _exercises.Max(e => e.Id) + 1,
            UserId = Guid.NewGuid(), // Mocking the authenticated user's ID
            MuscleGroupId = command.MuscleGroupId,
            Name = command.Name
        };

        _exercises.Add(newExercise);

        // In a real application, we would return a 201 Created with a URL to the new resource.
        // For this mock, we'll return the created object with a 201 status.
        return CreatedAtAction(nameof(GetExercises), new { id = newExercise.Id }, newExercise);
    }

    [HttpGet("{exerciseId}/previous-session")]
    public ActionResult<PreviousExercisePerformanceDto> GetPreviousExercisePerformance(int exerciseId)
    {
        // Mocking a check to see if the exercise exists
        if (!_exercises.Any(e => e.Id == exerciseId))
        {
            return NotFound("Exercise not found.");
        }

        // In a real implementation, you would query the database for the last performance.
        // For this mock, we'll return a hardcoded response.
        var previousPerformance = new PreviousExercisePerformanceDto
        {
            SessionDate = DateTime.UtcNow.AddDays(-7),
            Notes = "Previous notes on this exercise.",
            Sets = new List<PreviousExerciseSetDto>
            {
                new() { Weight = 95, Reps = 8, IsFailure = false },
                new() { Weight = 95, Reps = 8, IsFailure = false }
            }
        };

        return Ok(previousPerformance);
    }
}
