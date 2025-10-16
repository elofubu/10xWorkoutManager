using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.Data.Models;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface IMuscleGroupRepository
{
    Task<IEnumerable<MuscleGroup>> GetAllAsync();
    Task<MuscleGroup?> GetByIdAsync(int id);
}
