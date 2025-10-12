using Microsoft.AspNetCore.Mvc;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private static readonly List<SessionSummaryDto> _sessions = new()
    {
        new()
        {
            Id = 1,
            PlanId = 1,
            Notes = "Felt strong today.",
            StartTime = DateTime.UtcNow.AddDays(-1),
            EndTime = DateTime.UtcNow.AddDays(-1).AddHours(1),
            PlanName = "My Strength Plan",
            TrainingDayName = "Day A"
        },
        new()
        {
            Id = 2,
            PlanId = 1,
            Notes = "A bit tired.",
            StartTime = DateTime.UtcNow.AddDays(-3),
            EndTime = DateTime.UtcNow.AddDays(-3).AddHours(1),
            PlanName = "My Strength Plan",
            TrainingDayName = "Day B"
        }
    };

    [HttpGet]
    public ActionResult<PaginatedList<SessionSummaryDto>> GetSessions([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var totalCount = _sessions.Count;
        var paginatedData = _sessions.OrderByDescending(s => s.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = new PaginatedList<SessionSummaryDto>
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
    public ActionResult<SessionDetailsDto> GetSessionById(int id)
    {
        var sessionDetail = new SessionDetailsDto
        {
            Id = id,
            Notes = "Felt strong today.",
            StartTime = DateTime.UtcNow.AddDays(-1),
            EndTime = DateTime.UtcNow.AddDays(-1).AddHours(1),
            Exercises = new List<SessionExerciseDetailsDto>
            {
                new()
                {
                    Id = 1,
                    ExerciseId = 101,
                    Notes = "Good form.",
                    Skipped = false,
                    Order = 1,
                    Sets = new List<ExerciseSetDto>
                    {
                        new() { Id = 1, Weight = 100, Reps = 8, IsFailure = false, Order = 1 },
                        new() { Id = 2, Weight = 100, Reps = 7, IsFailure = true, Order = 2 }
                    }
                }
            }
        };

        // Mocking a check if the session exists
        if (!_sessions.Any(s => s.Id == id))
        {
            return NotFound();
        }

        return Ok(sessionDetail);
    }

    [HttpPost]
    public ActionResult<SessionDetailsDto> StartSession([FromBody] StartSessionCommand command)
    {
        // Mocking finding the training day and its exercises
        var trainingDayExercises = WorkoutPlansController._workoutPlanDetails
            .SelectMany(p => p.TrainingDays)
            .FirstOrDefault(td => td.Id == command.TrainingDayId)
            ?.Exercises;

        if (trainingDayExercises == null)
        {
            return NotFound("Training day not found.");
        }

        var newSessionId = _sessions.Max(s => s.Id) + 1;
        var newSession = new SessionDetailsDto
        {
            Id = newSessionId,
            Notes = null,
            StartTime = DateTime.UtcNow,
            EndTime = null,
            Exercises = trainingDayExercises.Select(e => new SessionExerciseDetailsDto
            {
                Id = new Random().Next(10, 100), // Mocked session exercise ID
                ExerciseId = e.ExerciseId,
                Order = e.Order,
                Sets = new List<ExerciseSetDto>()
            }).ToList()
        };
        
        // Add to the summary list
        _sessions.Add(new SessionSummaryDto
        {
            Id = newSessionId,
            PlanId = 0, // In a real app, you'd trace this back
            StartTime = newSession.StartTime,
            EndTime = newSession.EndTime
        });

        return CreatedAtAction(nameof(GetSessionById), new { id = newSessionId }, newSession);
    }

    [HttpPut("{id}")]
    public ActionResult<SessionSummaryDto> UpdateSession(int id, [FromBody] UpdateSessionCommand command)
    {
        var session = _sessions.FirstOrDefault(s => s.Id == id);
        if (session == null)
        {
            return NotFound();
        }

        // Apply updates from the command
        session.Notes = command.Notes ?? session.Notes;
        session.EndTime = command.EndTime ?? session.EndTime;

        return Ok(session);
    }
}
