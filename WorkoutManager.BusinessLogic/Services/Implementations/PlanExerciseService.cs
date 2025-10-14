using Supabase;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Exceptions;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class PlanExerciseService : IPlanExerciseService
{
    private readonly Client _supabaseClient;
    private readonly IWorkoutPlanService _workoutPlanService;

    public PlanExerciseService(Client supabaseClient, IWorkoutPlanService workoutPlanService)
    {
        _supabaseClient = supabaseClient;
        _workoutPlanService = workoutPlanService;
    }

    public async Task<CreatedPlanDayExerciseDto> AddExerciseToDayAsync(
        int planId,
        int dayId,
        AddExerciseToTrainingDayCommand command,
        Guid userId)
    {
        // Verify the plan exists and belongs to the user
        await VerifyPlanOwnershipAsync(planId, userId);

        // Check if plan is locked
        if (await _workoutPlanService.IsPlanLockedAsync(planId, userId))
        {
            throw new BusinessRuleViolationException("Cannot modify a workout plan that is currently being used in an active session.");
        }

        // Verify the training day exists and belongs to the plan
        var trainingDay = await _supabaseClient
            .From<TrainingDay>()
            .Where(td => td.Id == dayId)
            .Where(td => td.PlanId == planId)
            .Single();

        if (trainingDay == null)
        {
            throw new NotFoundException("TrainingDay", dayId);
        }

        // Verify the exercise exists
        var exercise = await _supabaseClient
            .From<Exercise>()
            .Where(e => e.Id == command.ExerciseId)
            .Single();

        if (exercise == null)
        {
            throw new NotFoundException("Exercise", command.ExerciseId);
        }

        // Create the plan day exercise
        var planDayExercise = new PlanDayExercise
        {
            TrainingDayId = dayId,
            ExerciseId = command.ExerciseId,
            Order = (short)command.Order
        };

        var response = await _supabaseClient
            .From<PlanDayExercise>()
            .Insert(planDayExercise);

        var created = response.Models.First();

        return new CreatedPlanDayExerciseDto(
            (int)created.Id,
            (int)created.TrainingDayId,
            (int)created.ExerciseId,
            created.Order
        );
    }

    public async Task RemoveExerciseFromDayAsync(int planId, int dayId, int planDayExerciseId, Guid userId)
    {
        // Verify the plan exists and belongs to the user
        await VerifyPlanOwnershipAsync(planId, userId);

        // Check if plan is locked
        if (await _workoutPlanService.IsPlanLockedAsync(planId, userId))
        {
            throw new BusinessRuleViolationException("Cannot modify a workout plan that is currently being used in an active session.");
        }

        // Verify the training day exists and belongs to the plan
        var trainingDay = await _supabaseClient
            .From<TrainingDay>()
            .Where(td => td.Id == dayId)
            .Where(td => td.PlanId == planId)
            .Single();

        if (trainingDay == null)
        {
            throw new NotFoundException("TrainingDay", dayId);
        }

        // Verify the plan day exercise exists and belongs to this training day
        var planDayExercise = await _supabaseClient
            .From<PlanDayExercise>()
            .Where(pde => pde.Id == planDayExerciseId)
            .Where(pde => pde.TrainingDayId == dayId)
            .Single();

        if (planDayExercise == null)
        {
            throw new NotFoundException("PlanDayExercise", planDayExerciseId);
        }

        // Delete the plan day exercise
        await _supabaseClient
            .From<PlanDayExercise>()
            .Where(pde => pde.Id == planDayExerciseId)
            .Delete();
    }

    public async Task ReorderExercisesAsync(int planId, int dayId, List<ReorderExerciseCommand> exercises, Guid userId)
    {
        // Verify the plan exists and belongs to the user
        await VerifyPlanOwnershipAsync(planId, userId);

        // Check if plan is locked
        if (await _workoutPlanService.IsPlanLockedAsync(planId, userId))
        {
            throw new BusinessRuleViolationException("Cannot modify a workout plan that is currently being used in an active session.");
        }

        // Verify the training day exists and belongs to the plan
        var trainingDay = await _supabaseClient
            .From<TrainingDay>()
            .Where(td => td.Id == dayId)
            .Where(td => td.PlanId == planId)
            .Single();

        if (trainingDay == null)
        {
            throw new NotFoundException("TrainingDay", dayId);
        }

        // Update the order for each exercise
        foreach (var exerciseUpdate in exercises)
        {
            var planDayExercise = await _supabaseClient
                .From<PlanDayExercise>()
                .Where(pde => pde.Id == exerciseUpdate.Id)
                .Where(pde => pde.TrainingDayId == dayId)
                .Single();

            if (planDayExercise != null)
            {
                planDayExercise.Order = (short)exerciseUpdate.Order;
                await _supabaseClient
                    .From<PlanDayExercise>()
                    .Update(planDayExercise);
            }
        }
    }

    private async Task VerifyPlanOwnershipAsync(int planId, Guid userId)
    {
        var plan = await _supabaseClient
            .From<WorkoutPlan>()
            .Where(wp => wp.Id == planId)
            .Where(wp => wp.UserId == userId)
            .Single();

        if (plan == null)
        {
            throw new NotFoundException("WorkoutPlan", planId);
        }
    }
}

