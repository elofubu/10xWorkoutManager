using System.Net;
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
            var result = await _httpClient.GetFromJsonAsync<PaginatedList<WorkoutPlanDto>>("api/workoutplans");
            return result ?? new PaginatedList<WorkoutPlanDto>();
        }

        public async Task<WorkoutPlanDetailDto?> GetWorkoutPlanByIdAsync(long id)
        {
            var response = await _httpClient.GetAsync($"api/workoutplans/{id}");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<WorkoutPlanDetailDto>();
        }

        public async Task CreateWorkoutPlanAsync(CreateWorkoutPlanDto newPlan)
        {
            var trainingDays = newPlan.TrainingDays?.Select(td => new CreateTrainingDayCommand(td.Name, td.Order)).ToList()
                ?? new List<CreateTrainingDayCommand>();
            var command = new CreateWorkoutPlanCommand(newPlan.Name, trainingDays);
            var response = await _httpClient.PostAsJsonAsync("api/workoutplans", command);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateWorkoutPlanAsync(long id, UpdateWorkoutPlanDto plan)
        {
            var payload = new UpdateWorkoutPlanPayload(plan.Name, plan.TrainingDays);
            var response = await _httpClient.PutAsJsonAsync($"api/workoutplans/{id}", payload);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteWorkoutPlanAsync(long id)
        {
            var response = await _httpClient.DeleteAsync($"api/workoutplans/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task AddExerciseToTrainingDayAsync(long planId, long dayId, AddExerciseToTrainingDayCommand exercise)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/workout-plans/{planId}/training-days/{dayId}/exercises", exercise);
            response.EnsureSuccessStatusCode();
        }

        public async Task RemoveExerciseFromTrainingDayAsync(long planId, long trainingDayId, long planDayExerciseId)
        {
            var response = await _httpClient.DeleteAsync($"api/workout-plans/{planId}/training-days/{trainingDayId}/exercises/{planDayExerciseId}");
            response.EnsureSuccessStatusCode();
        }

        public async Task ReorderExercisesAsync(long planId, long dayId, List<ReorderExerciseCommand> exercises)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/workout-plans/{planId}/training-days/{dayId}/exercises/reorder", exercises);
            response.EnsureSuccessStatusCode();
        }

        public Task ReorderTrainingDaysAsync(long planId, List<UpdateTrainingDayOrderCommand> days)
        {
            // Was already a no-op before this migration (see git history) — the BusinessLogic
            // method this needs doesn't exist on IWorkoutPlanService either. Left unimplemented,
            // same as before, but explicit now instead of silently doing nothing.
            throw new NotImplementedException(
                "Reordering training days requires business logic + an Api endpoint — neither exists yet.");
        }
    }
}
