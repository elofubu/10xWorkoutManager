using Supabase;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class PlanExerciseRepository : IPlanExerciseRepository
{
    private readonly Client _supabaseClient;

    public PlanExerciseRepository(Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<WorkoutPlan?> GetPlanByIdAndUserIdAsync(long planId, Guid userId)
    {
        return await _supabaseClient
            .From<WorkoutPlan>()
            .Where(wp => wp.Id == planId && wp.UserId == userId)
            .Single();
    }

    public async Task<TrainingDay?> GetTrainingDayByIdAndPlanIdAsync(long dayId, long planId)
    {
        return await _supabaseClient
            .From<TrainingDay>()
            .Where(td => td.Id == dayId && td.PlanId == planId)
            .Single();
    }

    public async Task<Exercise?> GetExerciseByIdAsync(long exerciseId)
    {
        return await _supabaseClient
            .From<Exercise>()
            .Where(e => e.Id == exerciseId)
            .Single();
    }

    public async Task<PlanDayExercise> AddExerciseToDayAsync(PlanDayExercise planDayExercise)
    {
        var response = await _supabaseClient
            .From<PlanDayExercise>()
            .Insert(planDayExercise);
        return response.Models[0];
    }

    public async Task<PlanDayExercise?> GetPlanDayExerciseAsync(long planDayExerciseId, long dayId)
    {
        return await _supabaseClient
            .From<PlanDayExercise>()
            .Where(pde => pde.Id == planDayExerciseId && pde.TrainingDayId == dayId)
            .Single();
    }

    public async Task RemoveExerciseFromDayAsync(long planDayExerciseId)
    {
        await _supabaseClient
            .From<PlanDayExercise>()
            .Where(pde => pde.Id == planDayExerciseId)
            .Delete();
    }

    public async Task ReorderExercisesAsync(long dayId, List<ReorderExerciseCommand> exercises)
    {
        foreach (var exerciseUpdate in exercises)
        {
            var planDayExercise = await _supabaseClient
                .From<PlanDayExercise>()
                .Where(pde => pde.Id == exerciseUpdate.Id && pde.TrainingDayId == dayId)
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
}


