using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface ISessionExerciseService
{
    Task<SessionExerciseDetailsDto> UpdateSessionExerciseAsync(
        int sessionId,
        int sessionExerciseId,
        UpdateSessionExerciseCommand command,
        Guid userId);
    
    Task MarkAsSkippedAsync(int sessionExerciseId, Guid userId);
}

