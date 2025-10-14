using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface IExerciseService
{
    Task<PaginatedList<ExerciseDto>> GetExercisesAsync(
        Guid userId,
        string? search = null,
        int? muscleGroupId = null,
        int page = 1,
        int pageSize = 20);
    
    Task<ExerciseDto?> GetExerciseByIdAsync(int exerciseId);
    
    Task<ExerciseDto> CreateExerciseAsync(CreateExerciseDto dto, Guid userId);
    
    Task<PreviousExercisePerformanceDto?> GetLastPerformanceAsync(int exerciseId, Guid userId);
}

