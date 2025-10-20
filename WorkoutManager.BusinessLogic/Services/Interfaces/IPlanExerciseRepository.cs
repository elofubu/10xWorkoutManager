using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface IPlanExerciseRepository
{
    Task<WorkoutPlan?> GetPlanByIdAndUserIdAsync(long planId, Guid userId);
    Task<TrainingDay?> GetTrainingDayByIdAndPlanIdAsync(long dayId, long planId);
    Task<Exercise?> GetExerciseByIdAsync(long exerciseId);
    Task<PlanDayExercise> AddExerciseToDayAsync(PlanDayExercise planDayExercise);
    Task<PlanDayExercise?> GetPlanDayExerciseAsync(long planDayExerciseId, long dayId);
    Task RemoveExerciseFromDayAsync(long planDayExerciseId);
    Task ReorderExercisesAsync(long dayId, List<ReorderExerciseCommand> exercises);

    // Get exercises with order information for a training day (M2M relation)
    Task<IEnumerable<(Exercise Exercise, short Order)>> GetExercisesWithOrderAsync(long trainingDayId);
}


