using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.Web.Services;

public interface IMuscleGroupService
{
    Task<PaginatedList<MuscleGroupDto>> GetMuscleGroupsAsync();
}

