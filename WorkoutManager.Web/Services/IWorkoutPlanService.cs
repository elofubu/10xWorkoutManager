using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Commands;

namespace WorkoutManager.Web.Services
{
    public interface IWorkoutPlanService
    {
        Task<PaginatedList<WorkoutPlanDto>> GetWorkoutPlansAsync();
        Task<WorkoutPlanDetailDto?> GetWorkoutPlanByIdAsync(long id);
        Task CreateWorkoutPlanAsync(CreateWorkoutPlanDto newPlan);
        Task UpdateWorkoutPlanAsync(long id, UpdateWorkoutPlanDto plan);
        Task DeleteWorkoutPlanAsync(long id);
        Task AddExerciseToTrainingDayAsync(long planId, long dayId, AddExerciseToTrainingDayCommand exercise);
        Task RemoveExerciseFromTrainingDayAsync(long planId, long trainingDayId, long planDayExerciseId);
        Task ReorderExercisesAsync(long planId, long dayId, List<ReorderExerciseCommand> exercises);
        Task ReorderTrainingDaysAsync(long planId, List<UpdateTrainingDayOrderCommand> days);
    }
}
