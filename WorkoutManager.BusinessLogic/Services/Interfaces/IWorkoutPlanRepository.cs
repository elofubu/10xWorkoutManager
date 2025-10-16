using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.Data.Models;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface IWorkoutPlanRepository
{
    Task<IEnumerable<WorkoutPlan>> GetWorkoutPlansAsync(Guid userId);
    Task<WorkoutPlan?> GetWorkoutPlanByIdAsync(long planId, Guid userId);
    Task<WorkoutPlan> CreateWorkoutPlanAsync(WorkoutPlan plan, IEnumerable<CreateTrainingDayCommand> trainingDays);
    Task UpdateWorkoutPlanAsync(WorkoutPlan plan, IEnumerable<UpdateTrainingDayOrderCommand> trainingDays);
    Task DeleteWorkoutPlanAsync(long planId);
    Task<bool> IsPlanLockedAsync(long planId, Guid userId);
    Task<IEnumerable<TrainingDay>> GetTrainingDaysWithExercisesAsync(long planId);
}
