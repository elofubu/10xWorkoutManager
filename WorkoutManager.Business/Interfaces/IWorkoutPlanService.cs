using WorkoutManager.Business.DTOs;

namespace WorkoutManager.Business.Interfaces
{
    public interface IWorkoutPlanService
    {
        Task<IEnumerable<WorkoutPlanDto>> GetAllWorkoutPlansAsync();
        Task<IEnumerable<WorkoutPlanSummaryDto>> GetAllWorkoutPlanSummariesAsync();
        Task<WorkoutPlanDto> GetWorkoutPlanByIdAsync(long id);
        Task<WorkoutPlanDto> CreateWorkoutPlanAsync(WorkoutPlanDto workoutPlanDto);
        Task UpdateWorkoutPlanAsync(long id, WorkoutPlanDto workoutPlanDto);
        Task DeleteWorkoutPlanAsync(long id);
    }
}
