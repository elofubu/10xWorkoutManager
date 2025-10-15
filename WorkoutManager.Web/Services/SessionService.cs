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
            this._logger = logger;
        }

        public async Task<PaginatedList<SessionSummaryDto>> GetSessionHistoryAsync()
        {
            return await _httpClient.GetFromJsonAsync<PaginatedList<SessionSummaryDto>>("api/sessions") ?? new PaginatedList<SessionSummaryDto>();
        }

        public async Task<SessionDetailsDto> GetSessionDetailsAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<SessionDetailsDto>($"api/sessions/{id}") ?? new SessionDetailsDto { Id = id };
        }

        public async Task<SessionDetailsDto> StartSessionAsync(int trainingDayId)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/sessions", new { trainingDayId });
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<SessionDetailsDto>() ?? new SessionDetailsDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return new SessionDetailsDto();
        }

        public async Task UpdateSessionExerciseAsync(int sessionId, int sessionExerciseId, UpdateSessionExerciseDto payload)
        {
            await _httpClient.PutAsJsonAsync($"api/sessions/{sessionId}/exercises/{sessionExerciseId}", payload);
        }

        public async Task UpdateSessionAsync(int sessionId, UpdateSessionCommand command)
        {
            await _httpClient.PutAsJsonAsync($"api/sessions/{sessionId}", command);
        }

        public async Task FinishSessionAsync(int sessionId, string? notes)
        {
            await _httpClient.PutAsJsonAsync($"api/sessions/{sessionId}", new UpdateSessionCommand
            {
                Notes = notes,
                EndTime = DateTime.UtcNow
            });
        }

        public async Task<SessionDetailsDto?> GetActiveSessionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/sessions/active");
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return null;
                }
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<SessionDetailsDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active session.");
                return null;
            }
        }
    }
}
