using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface ISessionService
{
    Task<SessionDetailsDto> StartSessionAsync(long trainingDayId, Guid userId);
    Task<PaginatedList<SessionSummaryDto>> GetSessionHistoryAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<SessionDetailsDto> GetSessionByIdAsync(long sessionId, Guid userId);
    Task UpdateSessionNotesAsync(long sessionId, string? notes, Guid userId);
    Task FinishSessionAsync(long sessionId, string? notes, Guid userId);
    Task<bool> HasActiveSessionAsync(Guid userId);
    Task<SessionDetailsDto?> GetActiveSessionAsync(Guid userId);
}

