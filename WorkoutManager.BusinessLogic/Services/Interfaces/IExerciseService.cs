using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface IExerciseService
{
    Task<PaginatedList<ExerciseDto>> GetExercisesAsync(
        Guid userId,
        string? search = null,
        long? muscleGroupId = null,
        int page = 1,
        int pageSize = 20);

    Task<ExerciseDto?> GetExerciseByIdAsync(long exerciseId);

    Task<ExerciseDto> CreateExerciseAsync(CreateExerciseDto dto, Guid userId);

    Task<PreviousExercisePerformanceDto?> GetLastPerformanceAsync(long exerciseId, Guid userId, long trainingDayId);
}

