using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Commands;
using BizLogic = WorkoutManager.BusinessLogic.Services.Interfaces;

namespace WorkoutManager.Web.Services
{
    public class SessionService : ISessionService
    {
        private readonly BizLogic.ISessionService _sessionService;
        private readonly BizLogic.ISessionExerciseService _sessionExerciseService;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILogger<SessionService> _logger;

        public SessionService(
            BizLogic.ISessionService sessionService,
            BizLogic.ISessionExerciseService sessionExerciseService,
            AuthenticationStateProvider authStateProvider,
            ILogger<SessionService> logger)
        {
            _sessionService = sessionService;
            _sessionExerciseService = sessionExerciseService;
            _authStateProvider = authStateProvider;
            _logger = logger;
        }

        private async Task<Guid> GetUserIdAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var userIdClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User not authenticated"));
        }

        public async Task<PaginatedList<SessionSummaryDto>> GetSessionHistoryAsync()
        {
            var userId = await GetUserIdAsync();
            return await _sessionService.GetSessionHistoryAsync(userId);
        }

        public async Task<SessionDetailsDto> GetSessionDetailsAsync(long id)
        {
            var userId = await GetUserIdAsync();
            return await _sessionService.GetSessionByIdAsync(id, userId);
        }

        public async Task<SessionDetailsDto> StartSessionAsync(long trainingDayId)
        {
            try
            {
                var userId = await GetUserIdAsync();
                return await _sessionService.StartSessionAsync(trainingDayId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting session");
                throw;
            }
        }

        public async Task UpdateSessionExerciseAsync(long sessionId, long sessionExerciseId, UpdateSessionExerciseDto payload)
        {
            try
            {
                var userId = await GetUserIdAsync();
                var command = new UpdateSessionExerciseCommand
                {
                    Notes = payload.Notes,
                    Skipped = payload.Skipped ?? false,
                    Sets = payload.Sets?.Select(s => new UpdateExerciseSetDto
                    {
                        Weight = s.Weight,
                        Reps = s.Reps,
                        IsFailure = false,
                        Order = 0
                    }).ToList() ?? new List<UpdateExerciseSetDto>()
                };
                await _sessionExerciseService.UpdateSessionExerciseAsync(sessionId, sessionExerciseId, command, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session exercise");
                throw;
            }
        }

        public async Task UpdateSessionAsync(long sessionId, UpdateSessionCommand command)
        {
            var userId = await GetUserIdAsync();
            await _sessionService.UpdateSessionNotesAsync(sessionId, command.Notes, userId);
        }

        public async Task FinishSessionAsync(long sessionId, string? notes)
        {
            var userId = await GetUserIdAsync();
            await _sessionService.FinishSessionAsync(sessionId, notes, userId);
        }

        public async Task<SessionDetailsDto?> GetActiveSessionAsync()
        {
            try
            {
                var userId = await GetUserIdAsync();
                return await _sessionService.GetActiveSessionAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active session");
                return null;
            }
        }
    }
}
