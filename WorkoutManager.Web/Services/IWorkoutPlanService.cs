using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Commands;

namespace WorkoutManager.Web.Services
{
    public interface IWorkoutPlanService
    {
        Task<PaginatedList<WorkoutPlanDto>> GetWorkoutPlansAsync();
        Task<WorkoutPlanDetailDto?> GetWorkoutPlanByIdAsync(int id);
        Task CreateWorkoutPlanAsync(CreateWorkoutPlanDto newPlan);
        Task UpdateWorkoutPlanAsync(int id, UpdateWorkoutPlanDto plan);
        Task DeleteWorkoutPlanAsync(int id);
        Task AddExerciseToTrainingDayAsync(int planId, int dayId, AddExerciseToTrainingDayCommand exercise);
        Task RemoveExerciseFromTrainingDayAsync(int planId, int trainingDayId, int planDayExerciseId);
        Task ReorderExercisesAsync(int planId, int dayId, List<ReorderExerciseCommand> exercises);
        Task ReorderTrainingDaysAsync(int planId, List<UpdateTrainingDayOrderCommand> days);
    }
}
