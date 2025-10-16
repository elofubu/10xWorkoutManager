using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Services.Interfaces;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class MuscleGroupService : IMuscleGroupService
{
    private readonly IMuscleGroupRepository _muscleGroupRepository;

    public MuscleGroupService(IMuscleGroupRepository muscleGroupRepository)
    {
        _muscleGroupRepository = muscleGroupRepository;
    }

    public async Task<IEnumerable<MuscleGroupDto>> GetAllMuscleGroupsAsync()
    {
        var muscleGroups = await _muscleGroupRepository.GetAllAsync();
        return muscleGroups.Select(mg => new MuscleGroupDto
        {
            Id = (int)mg.Id,
            Name = mg.Name
        }).ToList();
    }

    public async Task<MuscleGroupDto?> GetMuscleGroupByIdAsync(int id)
    {
        var muscleGroup = await _muscleGroupRepository.GetByIdAsync(id);
        if (muscleGroup == null) return null;

        return new MuscleGroupDto
        {
            Id = (int)muscleGroup.Id,
            Name = muscleGroup.Name
        };
    }
}

