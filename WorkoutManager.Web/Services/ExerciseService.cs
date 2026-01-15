using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using WorkoutManager.BusinessLogic.DTOs;
using BizLogic = WorkoutManager.BusinessLogic.Services.Interfaces;

namespace WorkoutManager.Web.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly BizLogic.IExerciseService _exerciseService;
        private readonly AuthenticationStateProvider _authStateProvider;

        public ExerciseService(
            BizLogic.IExerciseService exerciseService,
            AuthenticationStateProvider authStateProvider)
        {
            _exerciseService = exerciseService;
            _authStateProvider = authStateProvider;
        }

        private async Task<Guid> GetUserIdAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var userIdClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User not authenticated"));
        }

        public async Task<PaginatedList<ExerciseDto>> GetExercisesAsync(string? search = null, long? muscleGroupId = null, int page = 1)
        {
            var userId = await GetUserIdAsync();
            return await _exerciseService.GetExercisesAsync(userId, search, muscleGroupId, page);
        }

        public async Task<ExerciseDto> CreateExerciseAsync(CreateExerciseDto newExercise)
        {
            var userId = await GetUserIdAsync();
            return await _exerciseService.CreateExerciseAsync(newExercise, userId);
        }

        public async Task<PreviousExercisePerformanceDto?> GetPreviousSessionExerciseAsync(long exerciseId, long trainingDayId)
        {
            var userId = await GetUserIdAsync();
            return await _exerciseService.GetLastPerformanceAsync(exerciseId, userId, trainingDayId);
        }

        public async Task<ExerciseDto?> GetExerciseByIdAsync(long exerciseId)
        {
            return await _exerciseService.GetExerciseByIdAsync(exerciseId);
        }
    }
}
