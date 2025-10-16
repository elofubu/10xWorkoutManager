using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface IMuscleGroupService
{
    Task<IEnumerable<MuscleGroupDto>> GetAllMuscleGroupsAsync();
    Task<MuscleGroupDto?> GetMuscleGroupByIdAsync(long id);
}

