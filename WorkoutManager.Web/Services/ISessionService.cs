using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Commands;

namespace WorkoutManager.Web.Services
{
    public interface ISessionService
    {
        Task<PaginatedList<SessionSummaryDto>> GetSessionHistoryAsync();
        Task<SessionDetailsDto> GetSessionDetailsAsync(int id);
        Task<SessionDetailsDto> StartSessionAsync(int trainingDayId);
        Task UpdateSessionExerciseAsync(int sessionId, int sessionExerciseId, UpdateSessionExerciseDto payload);
        Task UpdateSessionAsync(int sessionId, UpdateSessionCommand command);
        Task FinishSessionAsync(int sessionId, string? notes);
    }
}
