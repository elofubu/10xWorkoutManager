using System.Net;
using System.Net.Http.Json;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Commands;

namespace WorkoutManager.Web.Services
{
    public class SessionService : ISessionService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SessionService> _logger;

        public SessionService(HttpClient httpClient, ILogger<SessionService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<PaginatedList<SessionSummaryDto>> GetSessionHistoryAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<PaginatedList<SessionSummaryDto>>("api/sessions");
            return result ?? new PaginatedList<SessionSummaryDto>();
        }

        public async Task<SessionDetailsDto> GetSessionDetailsAsync(long id)
        {
            var result = await _httpClient.GetFromJsonAsync<SessionDetailsDto>($"api/sessions/{id}");
            return result ?? throw new InvalidOperationException($"Api returned an empty response for session {id}.");
        }

        public async Task<SessionDetailsDto> StartSessionAsync(long trainingDayId)
        {
            try
            {
                var command = new StartSessionCommand { TrainingDayId = (int)trainingDayId };
                var response = await _httpClient.PostAsJsonAsync("api/sessions", command);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<SessionDetailsDto>();
                return result ?? throw new InvalidOperationException("Api returned an empty response for StartSession.");
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
                var response = await _httpClient.PutAsJsonAsync($"api/sessions/{sessionId}/exercises/{sessionExerciseId}", command);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session exercise");
                throw;
            }
        }

        public async Task UpdateSessionAsync(long sessionId, UpdateSessionCommand command)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/sessions/{sessionId}", command);
            response.EnsureSuccessStatusCode();
        }

        public async Task FinishSessionAsync(long sessionId, string? notes)
        {
            // The Api's PUT api/sessions/{id} branches on EndTime: set -> finish, unset -> just update notes.
            var command = new UpdateSessionCommand { Notes = notes, EndTime = DateTime.UtcNow };
            var response = await _httpClient.PutAsJsonAsync($"api/sessions/{sessionId}", command);
            response.EnsureSuccessStatusCode();
        }

        public async Task<SessionDetailsDto?> GetActiveSessionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/sessions/active");
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return null;
                }
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<SessionDetailsDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active session");
                return null;
            }
        }

        public async Task DeleteSessionAsync(long sessionId)
        {
            try
            {
                // NOTE: requires WorkoutManager.Api to expose DELETE api/sessions/{id} — see
                // the Api-side addition made alongside this Phase 4 change.
                var response = await _httpClient.DeleteAsync($"api/sessions/{sessionId}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting session {SessionId}", sessionId);
                throw;
            }
        }
    }
}
