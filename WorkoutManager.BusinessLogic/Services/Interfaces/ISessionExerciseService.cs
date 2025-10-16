using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface ISessionExerciseService
{
    Task<SessionExerciseDetailsDto> UpdateSessionExerciseAsync(
        long sessionId,
        long sessionExerciseId,
        UpdateSessionExerciseCommand command,
        Guid userId);

    Task MarkAsSkippedAsync(long sessionExerciseId, Guid userId);
}

