using Microsoft.AspNetCore.Mvc;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkoutPlansController : ControllerBase
{
    internal static readonly List<WorkoutPlanDetailDto> _workoutPlanDetails = new()
    {
        new
        (
            Id: 1,
            Name: "My Strength Plan",
            IsLocked: false,
            TrainingDays: new List<TrainingDayDto>
            {
                new(Id: 1, Name: "Day A", Order: 1, Exercises: new List<PlanDayExerciseDto>
                {
                    new(1, 101, "Bench Press", 1),
                    new(2, 104, "Bent Over Row", 2)
                }),
                new(Id: 2, Name: "Day B", Order: 2, Exercises: new List<PlanDayExerciseDto>
                {
                    new(3, 103, "Pull Up", 1),
                    new(4, 102, "Incline Dumbbell Press", 2)
                })
            }
        ),
        new
        (
            Id: 2,
            Name: "Hypertrophy Program",
            IsLocked: true, // Mocked as locked
            TrainingDays: new List<TrainingDayDto>()
        )
    };
    
    // This list is derived for the GET /workout-plans endpoint.
    private static readonly List<WorkoutPlanDto> _workoutPlans = new()
    {
        new(1, "My Strength Plan", DateTime.UtcNow.AddDays(-10)),
        new(2, "Hypertrophy Program", DateTime.UtcNow.AddDays(-5)),
    };

    [HttpGet]
    public ActionResult<PaginatedList<WorkoutPlanDto>> GetWorkoutPlans([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var totalCount = _workoutPlans.Count;
        var paginatedData = _workoutPlans.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var result = new PaginatedList<WorkoutPlanDto>
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

    [HttpGet("{id}")]
    public ActionResult<WorkoutPlanDetailDto> GetWorkoutPlanById(int id)
    {
        var planDetail = _workoutPlanDetails.FirstOrDefault(p => p.Id == id);

        if (planDetail == null)
        {
            return NotFound();
        }

        return Ok(planDetail);
    }

    [HttpPost]
    public ActionResult<CreatedWorkoutPlanDto> CreateWorkoutPlan([FromBody] CreateWorkoutPlanCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return BadRequest("Workout plan name cannot be empty.");
        }

        var newPlanId = _workoutPlans.Max(p => p.Id) + 1;
        
        var newPlan = new WorkoutPlanDto(
            Id: newPlanId,
            Name: command.Name,
            CreatedAt: DateTime.UtcNow
        );
        _workoutPlans.Add(newPlan);

        var createdTrainingDays = command.TrainingDays
            .Select(td => new CreatedTrainingDayDto(Id: new Random().Next(10, 100), td.Name, td.Order))
            .ToList();

        var responseDto = new CreatedWorkoutPlanDto(
            Id: newPlanId,
            Name: command.Name,
            TrainingDays: createdTrainingDays
        );
        
        // Also add to the detailed list for consistency in the mock
        _workoutPlanDetails.Add(new WorkoutPlanDetailDto(newPlanId, command.Name, false, new List<TrainingDayDto>()));

        return CreatedAtAction(nameof(GetWorkoutPlanById), new { id = newPlanId }, responseDto);
    }

    [HttpPut("{id}")]
    public ActionResult<WorkoutPlanDetailDto> UpdateWorkoutPlan(int id, [FromBody] UpdateWorkoutPlanPayload payload)
    {
        var planDetail = _workoutPlanDetails.FirstOrDefault(p => p.Id == id);
        if (planDetail == null)
        {
            return NotFound();
        }

        if (planDetail.IsLocked)
        {
            return Forbid();
        }

        // In a real implementation, you would update the object from the payload.
        // Here, we'll just update the name and return the object.
        var updatedPlan = planDetail with { Name = payload.Name };
        
        // Replace in mock list
        var index = _workoutPlanDetails.FindIndex(p => p.Id == id);
        _workoutPlanDetails[index] = updatedPlan;

        return Ok(updatedPlan);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteWorkoutPlan(int id)
    {
        var planDetail = _workoutPlanDetails.FirstOrDefault(p => p.Id == id);
        if (planDetail == null)
        {
            return NotFound();
        }

        if (planDetail.IsLocked)
        {
            return Forbid();
        }

        _workoutPlans.RemoveAll(p => p.Id == id);
        _workoutPlanDetails.RemoveAll(p => p.Id == id);

        return NoContent();
    }
}
