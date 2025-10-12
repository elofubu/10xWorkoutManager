using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.Web.Services
{
    public interface IExerciseService
    {
        Task<PaginatedList<ExerciseDto>> GetExercisesAsync(string? search = null, int? muscleGroupId = null);
        Task<ExerciseDto> CreateExerciseAsync(CreateExerciseDto newExercise);
        Task<PreviousExercisePerformanceDto?> GetPreviousSessionExerciseAsync(int exerciseId);
        Task<ExerciseDto?> GetExerciseByIdAsync(int exerciseId);
    }
}
