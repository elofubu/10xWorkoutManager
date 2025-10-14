using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface IMuscleGroupService
{
    Task<PaginatedList<MuscleGroupDto>> GetAllMuscleGroupsAsync(int page = 1, int pageSize = 20);
    Task<MuscleGroupDto?> GetMuscleGroupByIdAsync(int id);
}

