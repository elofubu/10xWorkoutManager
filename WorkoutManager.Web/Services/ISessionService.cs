using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Commands;

namespace WorkoutManager.Web.Services
{
    public interface ISessionService
    {
        Task<PaginatedList<SessionSummaryDto>> GetSessionHistoryAsync();
        Task<SessionDetailsDto> GetSessionDetailsAsync(long id);
        Task<SessionDetailsDto> StartSessionAsync(long trainingDayId);
        Task UpdateSessionExerciseAsync(long sessionId, long sessionExerciseId, UpdateSessionExerciseDto payload);
        Task UpdateSessionAsync(long sessionId, UpdateSessionCommand command);
        Task FinishSessionAsync(long sessionId, string? notes);
        Task<SessionDetailsDto?> GetActiveSessionAsync();
    }
}
