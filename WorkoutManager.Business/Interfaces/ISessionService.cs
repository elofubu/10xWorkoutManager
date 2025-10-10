using WorkoutManager.Business.DTOs;

namespace WorkoutManager.Business.Interfaces
{
    public interface ISessionService
    {
        Task<IEnumerable<SessionSummaryDto>> GetSessionHistoryAsync();
        Task<SessionDto> GetLatestSessionAsync();
        Task<SessionDto> GetSessionByIdAsync(long id);
        Task<SessionDto> StartSessionAsync(long? planId);
        Task EndSessionAsync(long sessionId);
        Task AddExerciseToSessionAsync(long sessionId, long exerciseId);
        Task RemoveExerciseFromSessionAsync(long sessionId, long sessionExerciseId);
        Task AddSetToSessionExerciseAsync(long sessionExerciseId, ExerciseSetDto setDto);
        Task RemoveSetFromSessionExerciseAsync(long sessionExerciseId, long setId);
        Task UpdateSetAsync(long setId, ExerciseSetDto setDto);
        Task SkipExerciseAsync(long sessionExerciseId);
    }
}
