using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.Data.Models;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface IExerciseRepository
{
    Task<IEnumerable<Exercise>> GetExercisesForUserAsync(Guid userId);
    Task<Exercise?> GetExerciseByIdAsync(long exerciseId);
    Task<Exercise?> GetExerciseByNameForUserAsync(string name, Guid userId);
    Task<Exercise> CreateExerciseAsync(Exercise exercise);
    Task<PreviousExercisePerformanceDto?> GetLastPerformanceAsync(long exerciseId, Guid userId, long trainingDayId);
}
