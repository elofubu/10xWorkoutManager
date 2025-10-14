using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface IPlanExerciseService
{
    Task<CreatedPlanDayExerciseDto> AddExerciseToDayAsync(
        int planId,
        int dayId,
        AddExerciseToTrainingDayCommand command,
        Guid userId);
    
    Task RemoveExerciseFromDayAsync(int planId, int dayId, int planDayExerciseId, Guid userId);
    
    Task ReorderExercisesAsync(int planId, int dayId, List<ReorderExerciseCommand> exercises, Guid userId);
}

