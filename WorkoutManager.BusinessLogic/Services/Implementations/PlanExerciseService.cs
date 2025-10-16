using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Exceptions;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class PlanExerciseService : IPlanExerciseService
{
    private readonly IPlanExerciseRepository _planExerciseRepository;
    private readonly IWorkoutPlanService _workoutPlanService;

    public PlanExerciseService(IPlanExerciseRepository planExerciseRepository, IWorkoutPlanService workoutPlanService)
    {
        _planExerciseRepository = planExerciseRepository;
        _workoutPlanService = workoutPlanService;
    }

    public async Task<CreatedPlanDayExerciseDto> AddExerciseToDayAsync(
        int planId,
        int dayId,
        AddExerciseToTrainingDayCommand command,
        Guid userId)
    {
        await VerifyPlanOwnershipAsync(planId, userId);
        if (await _workoutPlanService.IsPlanLockedAsync(planId, userId))
        {
            throw new BusinessRuleViolationException("Cannot modify a workout plan that is currently being used in an active session.");
        }
        
        var trainingDay = await _planExerciseRepository.GetTrainingDayByIdAndPlanIdAsync(dayId, planId);
        if (trainingDay == null)
        {
            throw new NotFoundException("TrainingDay", dayId);
        }

        var exercise = await _planExerciseRepository.GetExerciseByIdAsync(command.ExerciseId);
        if (exercise == null)
        {
            throw new NotFoundException("Exercise", command.ExerciseId);
        }

        var planDayExercise = new PlanDayExercise
        {
            TrainingDayId = dayId,
            ExerciseId = command.ExerciseId,
            Order = (short)command.Order
        };

        var created = await _planExerciseRepository.AddExerciseToDayAsync(planDayExercise);

        return new CreatedPlanDayExerciseDto(
            (int)created.Id,
            (int)created.TrainingDayId,
            (int)created.ExerciseId,
            created.Order
        );
    }

    public async Task RemoveExerciseFromDayAsync(int planId, int dayId, int planDayExerciseId, Guid userId)
    {
        await VerifyPlanOwnershipAsync(planId, userId);
        if (await _workoutPlanService.IsPlanLockedAsync(planId, userId))
        {
            throw new BusinessRuleViolationException("Cannot modify a workout plan that is currently being used in an active session.");
        }
        
        var trainingDay = await _planExerciseRepository.GetTrainingDayByIdAndPlanIdAsync(dayId, planId);
        if (trainingDay == null)
        {
            throw new NotFoundException("TrainingDay", dayId);
        }

        var planDayExercise = await _planExerciseRepository.GetPlanDayExerciseAsync(planDayExerciseId, dayId);
        if (planDayExercise == null)
        {
            throw new NotFoundException("PlanDayExercise", planDayExerciseId);
        }
        
        await _planExerciseRepository.RemoveExerciseFromDayAsync(planDayExerciseId);
    }

    public async Task ReorderExercisesAsync(int planId, int dayId, List<ReorderExerciseCommand> exercises, Guid userId)
    {
        await VerifyPlanOwnershipAsync(planId, userId);
        if (await _workoutPlanService.IsPlanLockedAsync(planId, userId))
        {
            throw new BusinessRuleViolationException("Cannot modify a workout plan that is currently being used in an active session.");
        }
        
        var trainingDay = await _planExerciseRepository.GetTrainingDayByIdAndPlanIdAsync(dayId, planId);
        if (trainingDay == null)
        {
            throw new NotFoundException("TrainingDay", dayId);
        }
        
        await _planExerciseRepository.ReorderExercisesAsync(dayId, exercises);
    }

    private async Task VerifyPlanOwnershipAsync(int planId, Guid userId)
    {
        var plan = await _planExerciseRepository.GetPlanByIdAndUserIdAsync(planId, userId);
        if (plan == null)
        {
            throw new NotFoundException("WorkoutPlan", planId);
        }
    }
}

