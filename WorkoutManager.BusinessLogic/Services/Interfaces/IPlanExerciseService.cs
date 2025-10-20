using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface IPlanExerciseService
{
    Task<CreatedPlanDayExerciseDto> AddExerciseToDayAsync(
        long planId,
        long dayId,
        AddExerciseToTrainingDayCommand command,
        Guid userId);

    Task RemoveExerciseFromDayAsync(long planId, long dayId, long planDayExerciseId, Guid userId);

    Task ReorderExercisesAsync(long planId, long dayId, List<ReorderExerciseCommand> exercises, Guid userId);

    // Get exercises for a training day with order information (M2M relation)
    Task<IEnumerable<PlanExerciseDetailDto>> GetExercisesForTrainingDayAsync(long trainingDayId);
}

