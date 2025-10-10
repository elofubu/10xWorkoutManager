using WorkoutManager.Business.DTOs;

namespace WorkoutManager.Business.Interfaces
{
    public interface IExerciseService
    {
        Task<IEnumerable<ExerciseDto>> GetAllExercisesAsync();
        Task<ExerciseDto> GetExerciseByIdAsync(long id);
        Task<ExerciseDto> CreateExerciseAsync(CreateExerciseDto exerciseDto);
        Task UpdateExerciseAsync(long id, ExerciseDto exerciseDto);
        Task DeleteExerciseAsync(long id);
        Task<IEnumerable<MuscleGroupDto>> GetAllMuscleGroupsAsync();
        Task<IEnumerable<ExerciseHistoryDto>> GetExerciseHistoryAsync(long exerciseId);
    }
}
