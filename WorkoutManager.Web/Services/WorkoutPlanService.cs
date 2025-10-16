using System.Net.Http.Json;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Commands;

namespace WorkoutManager.Web.Services
{
    public class WorkoutPlanService : IWorkoutPlanService
    {
        private readonly HttpClient _httpClient;

        public WorkoutPlanService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PaginatedList<WorkoutPlanDto>> GetWorkoutPlansAsync()
        {
            return await _httpClient.GetFromJsonAsync<PaginatedList<WorkoutPlanDto>>("api/workoutplans") ?? new PaginatedList<WorkoutPlanDto>();
        }

        public async Task<WorkoutPlanDetailDto?> GetWorkoutPlanByIdAsync(long id)
        {
            return await _httpClient.GetFromJsonAsync<WorkoutPlanDetailDto>($"api/workoutplans/{id}");
        }

        public async Task CreateWorkoutPlanAsync(CreateWorkoutPlanDto newPlan)
        {
            await _httpClient.PostAsJsonAsync("api/workoutplans", newPlan);
        }

        public async Task UpdateWorkoutPlanAsync(long id, UpdateWorkoutPlanDto plan)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/workoutplans/{id}", plan);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteWorkoutPlanAsync(long id)
        {
            var response = await _httpClient.DeleteAsync($"api/workoutplans/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task AddExerciseToTrainingDayAsync(long planId, long dayId, AddExerciseToTrainingDayCommand exercise)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/workout-plans/{planId}/training-days/{dayId}/exercises", exercise);
            response.EnsureSuccessStatusCode();
        }

        public async Task RemoveExerciseFromTrainingDayAsync(long planId, long trainingDayId, long planDayExerciseId)
        {
            await _httpClient.DeleteAsync($"api/workout-plans/{planId}/training-days/{trainingDayId}/exercises/{planDayExerciseId}");
        }

        public async Task ReorderExercisesAsync(long planId, long dayId, List<ReorderExerciseCommand> exercises)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/workout-plans/{planId}/training-days/{dayId}/exercises/reorder", exercises);
            response.EnsureSuccessStatusCode();
        }

        public async Task ReorderTrainingDaysAsync(long planId, List<UpdateTrainingDayOrderCommand> days)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/workout-plans/{planId}/training-days/reorder", days);
            response.EnsureSuccessStatusCode();
        }
    }
}
