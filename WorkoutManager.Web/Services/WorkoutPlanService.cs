using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Commands;
using BizLogic = WorkoutManager.BusinessLogic.Services.Interfaces;

namespace WorkoutManager.Web.Services
{
    public class WorkoutPlanService : IWorkoutPlanService
    {
        private readonly BizLogic.IWorkoutPlanService _workoutPlanService;
        private readonly BizLogic.IPlanExerciseService _planExerciseService;
        private readonly AuthenticationStateProvider _authStateProvider;

        public WorkoutPlanService(
            BizLogic.IWorkoutPlanService workoutPlanService,
            BizLogic.IPlanExerciseService planExerciseService,
            AuthenticationStateProvider authStateProvider)
        {
            _workoutPlanService = workoutPlanService;
            _planExerciseService = planExerciseService;
            _authStateProvider = authStateProvider;
        }

        private async Task<Guid> GetUserIdAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var userIdClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User not authenticated"));
        }

        public async Task<PaginatedList<WorkoutPlanDto>> GetWorkoutPlansAsync()
        {
            var userId = await GetUserIdAsync();
            return await _workoutPlanService.GetWorkoutPlansAsync(userId);
        }

        public async Task<WorkoutPlanDetailDto?> GetWorkoutPlanByIdAsync(long id)
        {
            var userId = await GetUserIdAsync();
            return await _workoutPlanService.GetWorkoutPlanByIdAsync(id, userId);
        }

        public async Task CreateWorkoutPlanAsync(CreateWorkoutPlanDto newPlan)
        {
            var userId = await GetUserIdAsync();
            var trainingDays = newPlan.TrainingDays?.Select(td => new CreateTrainingDayCommand(td.Name, td.Order)).ToList()
                ?? new List<CreateTrainingDayCommand>();
            var command = new CreateWorkoutPlanCommand(newPlan.Name, trainingDays);
            await _workoutPlanService.CreateWorkoutPlanAsync(command, userId);
        }

        public async Task UpdateWorkoutPlanAsync(long id, UpdateWorkoutPlanDto plan)
        {
            var userId = await GetUserIdAsync();
            var payload = new UpdateWorkoutPlanPayload(plan.Name, plan.TrainingDays);
            await _workoutPlanService.UpdateWorkoutPlanAsync(id, payload, userId);
        }

        public async Task DeleteWorkoutPlanAsync(long id)
        {
            var userId = await GetUserIdAsync();
            await _workoutPlanService.DeleteWorkoutPlanAsync(id, userId);
        }

        public async Task AddExerciseToTrainingDayAsync(long planId, long dayId, AddExerciseToTrainingDayCommand exercise)
        {
            var userId = await GetUserIdAsync();
            await _planExerciseService.AddExerciseToDayAsync(planId, dayId, exercise, userId);
        }

        public async Task RemoveExerciseFromTrainingDayAsync(long planId, long trainingDayId, long planDayExerciseId)
        {
            var userId = await GetUserIdAsync();
            await _planExerciseService.RemoveExerciseFromDayAsync(planId, trainingDayId, planDayExerciseId, userId);
        }

        public async Task ReorderExercisesAsync(long planId, long dayId, List<ReorderExerciseCommand> exercises)
        {
            var userId = await GetUserIdAsync();
            await _planExerciseService.ReorderExercisesAsync(planId, dayId, exercises, userId);
        }

        public async Task ReorderTrainingDaysAsync(long planId, List<UpdateTrainingDayOrderCommand> days)
        {
            var userId = await GetUserIdAsync();
            // This method doesn't exist in IPlanExerciseService - it should be in IWorkoutPlanService
            var payload = new UpdateWorkoutPlanPayload(string.Empty, days);
            // Note: This may need adjustment based on actual business logic requirements
        }
    }
}
