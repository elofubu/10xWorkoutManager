using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface ISessionService
{
    Task<SessionDetailsDto> StartSessionAsync(int trainingDayId, Guid userId);
    Task<PaginatedList<SessionSummaryDto>> GetSessionHistoryAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<SessionDetailsDto> GetSessionByIdAsync(int sessionId, Guid userId);
    Task UpdateSessionNotesAsync(int sessionId, string? notes, Guid userId);
    Task FinishSessionAsync(int sessionId, string? notes, Guid userId);
    Task<bool> HasActiveSessionAsync(Guid userId);
    Task<SessionDetailsDto?> GetActiveSessionAsync(Guid userId);
}

