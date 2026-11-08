using System.Net.Http.Json;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.Web.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly HttpClient _httpClient;

        public ExerciseService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PaginatedList<ExerciseDto>> GetExercisesAsync(string? search = null, long? muscleGroupId = null, int page = 1)
        {
            var queryParams = new List<string> { $"page={page}" };
            if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
            if (muscleGroupId.HasValue) queryParams.Add($"muscleGroupId={muscleGroupId.Value}");

            var result = await _httpClient.GetFromJsonAsync<PaginatedList<ExerciseDto>>($"api/exercises?{string.Join("&", queryParams)}");
            return result ?? new PaginatedList<ExerciseDto>();
        }

        public async Task<ExerciseDto> CreateExerciseAsync(CreateExerciseDto newExercise)
        {
            var response = await _httpClient.PostAsJsonAsync("api/exercises", newExercise);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ExerciseDto>();
            return result ?? throw new InvalidOperationException("Api returned an empty response for CreateExercise.");
        }

        public async Task<PreviousExercisePerformanceDto?> GetPreviousSessionExerciseAsync(long exerciseId, long trainingDayId)
        {
            var response = await _httpClient.GetAsync($"api/exercises/{trainingDayId}/{exerciseId}/previous-session");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PreviousExercisePerformanceDto>();
        }

        public async Task<ExerciseDto?> GetExerciseByIdAsync(long exerciseId)
        {
            var response = await _httpClient.GetAsync($"api/exercises/{exerciseId}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ExerciseDto>();
        }
    }
}
