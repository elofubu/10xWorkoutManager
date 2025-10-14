using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface IWorkoutPlanService
{
    Task<PaginatedList<WorkoutPlanDto>> GetWorkoutPlansAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<WorkoutPlanDetailDto> GetWorkoutPlanByIdAsync(int planId, Guid userId);
    Task<CreatedWorkoutPlanDto> CreateWorkoutPlanAsync(CreateWorkoutPlanCommand command, Guid userId);
    Task UpdateWorkoutPlanAsync(int planId, UpdateWorkoutPlanPayload payload, Guid userId);
    Task DeleteWorkoutPlanAsync(int planId, Guid userId);
    Task<bool> IsPlanLockedAsync(int planId, Guid userId);
}

