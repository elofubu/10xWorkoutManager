using System.Net.Http.Json;
using System.Web;
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
            if (_httpClient.BaseAddress is null)
            {
                return new PaginatedList<ExerciseDto>();
            }

            var uriBuilder = new UriBuilder(_httpClient.BaseAddress)
            {
                Path = "api/exercises"
            };
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            if (!string.IsNullOrWhiteSpace(search))
            {
                query["search"] = search;
            }
            if (muscleGroupId.HasValue)
            {
                query["muscleGroupId"] = muscleGroupId.Value.ToString();
            }
            if (page > 1)
            {
                query["page"] = page.ToString();
            }
            uriBuilder.Query = query.ToString();

            return await _httpClient.GetFromJsonAsync<PaginatedList<ExerciseDto>>(uriBuilder.ToString()) ?? new PaginatedList<ExerciseDto>();
        }

        public async Task<ExerciseDto> CreateExerciseAsync(CreateExerciseDto newExercise)
        {
            var response = await _httpClient.PostAsJsonAsync("api/exercises", newExercise);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ExerciseDto>() ?? throw new Exception("Failed to create exercise.");
        }

        public async Task<PreviousExercisePerformanceDto?> GetPreviousSessionExerciseAsync(long exerciseId, long trainingDayId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<PreviousExercisePerformanceDto>($"api/exercises/{trainingDayId}/{exerciseId}/previous-session");
            }
            catch (HttpRequestException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<ExerciseDto?> GetExerciseByIdAsync(long exerciseId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ExerciseDto>($"api/exercises/{exerciseId}");
            }
            catch (HttpRequestException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }
    }
}
