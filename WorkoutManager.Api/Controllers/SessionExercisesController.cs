using Microsoft.AspNetCore.Mvc;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.Api.Controllers;

[ApiController]
[Route("api/sessions/{sessionId}/exercises")]
public class SessionExercisesController : ControllerBase
{
    [HttpPut("{sessionExerciseId}")]
    public ActionResult<SessionExerciseDetailsDto> UpdateSessionExercise(int sessionId, int sessionExerciseId, [FromBody] UpdateSessionExerciseCommand command)
    {
        // In a real application, you would find the session and the specific exercise.
        // For this mock, we'll assume the session and exercise exist and just return a mocked response.

        var updatedExercise = new SessionExerciseDetailsDto
        {
            Id = sessionExerciseId,
            ExerciseId = 0, // Mocked
            Notes = command.Notes,
            Skipped = command.Skipped,
            Order = 0, // Mocked
            Sets = command.Sets.Select(s => new ExerciseSetDto
            {
                Id = new Random().Next(100, 200), // Mocked set ID
                Weight = s.Weight,
                Reps = s.Reps,
                IsFailure = s.IsFailure,
                Order = s.Order
            }).ToList()
        };

        return Ok(updatedExercise);
    }
}
