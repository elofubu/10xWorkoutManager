using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface IWorkoutPlanService
{
    Task<PaginatedList<WorkoutPlanDto>> GetWorkoutPlansAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<WorkoutPlanDetailDto> GetWorkoutPlanByIdAsync(long planId, Guid userId);
    Task<CreatedWorkoutPlanDto> CreateWorkoutPlanAsync(CreateWorkoutPlanCommand command, Guid userId);
    Task UpdateWorkoutPlanAsync(long planId, UpdateWorkoutPlanPayload payload, Guid userId);
    Task DeleteWorkoutPlanAsync(long planId, Guid userId);
    Task<bool> IsPlanLockedAsync(long planId, Guid userId);
}

