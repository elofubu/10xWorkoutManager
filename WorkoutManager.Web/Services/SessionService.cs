using System.Net.Http.Json;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Commands;
using System.Net;

namespace WorkoutManager.Web.Services
{
    public class SessionService : ISessionService
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthService _authService;
        private readonly ILogger<SessionService> _logger;

        public SessionService(HttpClient httpClient, 
            IAuthService authService,
            ILogger<SessionService> logger)
        {
            _httpClient = httpClient;
            this._authService = authService;
            this._logger = logger;
        }

        public async Task<PaginatedList<SessionSummaryDto>> GetSessionHistoryAsync()
        {
            return await _httpClient.GetFromJsonAsync<PaginatedList<SessionSummaryDto>>("api/sessions") ?? new PaginatedList<SessionSummaryDto>();
        }

        public async Task<SessionDetailsDto> GetSessionDetailsAsync(long id)
        {
            return await _httpClient.GetFromJsonAsync<SessionDetailsDto>($"api/sessions/{id}") ?? new SessionDetailsDto { Id = id };
        }

        public async Task<SessionDetailsDto> StartSessionAsync(long trainingDayId)
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

        public async Task UpdateSessionExerciseAsync(long sessionId, long sessionExerciseId, UpdateSessionExerciseDto payload)
        {
            var result = await _httpClient.PutAsJsonAsync($"api/sessions/{sessionId}/exercises/{sessionExerciseId}", payload);

            if (result.StatusCode.Equals(System.Net.HttpStatusCode.Forbidden))
            {
                _logger.LogError("Response result is {statusCode}, trying to refresh token", HttpStatusCode.Forbidden);
                await _authService.RefreshToken();
                await _httpClient.PutAsJsonAsync($"api/sessions/{sessionId}/exercises/{sessionExerciseId}", payload);
            }
        }

        public async Task UpdateSessionAsync(long sessionId, UpdateSessionCommand command)
        {
            await _httpClient.PutAsJsonAsync($"api/sessions/{sessionId}", command);
        }

        public async Task FinishSessionAsync(long sessionId, string? notes)
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
