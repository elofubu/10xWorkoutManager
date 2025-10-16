using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.Web.Services
{
    public interface IExerciseService
    {
        Task<PaginatedList<ExerciseDto>> GetExercisesAsync(string? search = null, long? muscleGroupId = null, int page = 1);
        Task<ExerciseDto> CreateExerciseAsync(CreateExerciseDto newExercise);
        Task<PreviousExercisePerformanceDto?> GetPreviousSessionExerciseAsync(long exerciseId);
        Task<ExerciseDto?> GetExerciseByIdAsync(long exerciseId);
    }
}
