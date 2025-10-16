using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface IPlanExerciseRepository
{
    Task<WorkoutPlan?> GetPlanByIdAndUserIdAsync(int planId, Guid userId);
    Task<TrainingDay?> GetTrainingDayByIdAndPlanIdAsync(int dayId, int planId);
    Task<Exercise?> GetExerciseByIdAsync(int exerciseId);
    Task<PlanDayExercise> AddExerciseToDayAsync(PlanDayExercise planDayExercise);
    Task<PlanDayExercise?> GetPlanDayExerciseAsync(int planDayExerciseId, int dayId);
    Task RemoveExerciseFromDayAsync(int planDayExerciseId);
    Task ReorderExercisesAsync(int dayId, List<ReorderExerciseCommand> exercises);
}


