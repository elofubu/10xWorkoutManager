using Supabase;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class WorkoutPlanRepository : IWorkoutPlanRepository
{
    private readonly Client _supabaseClient;

    public WorkoutPlanRepository(Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<IEnumerable<WorkoutPlan>> GetWorkoutPlansAsync(Guid userId)
    {
        var response = await _supabaseClient
            .From<WorkoutPlan>()
            .Where(wp => wp.UserId == userId)
            .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
            .Get();
        return response.Models;
    }

    public async Task<WorkoutPlan?> GetWorkoutPlanByIdAsync(long planId, Guid userId)
    {
        return await _supabaseClient
            .From<WorkoutPlan>()
            .Where(wp => wp.Id == planId && wp.UserId == userId)
            .Single();
    }

    public async Task<WorkoutPlan> CreateWorkoutPlanAsync(WorkoutPlan plan, IEnumerable<CreateTrainingDayCommand> trainingDays)
    {
        var planResponse = await _supabaseClient
            .From<WorkoutPlan>()
            .Insert(plan);
        var createdPlan = planResponse.Models.First();

        foreach (var dayCommand in trainingDays)
        {
            var trainingDay = new TrainingDay
            {
                PlanId = createdPlan.Id,
                Name = dayCommand.Name,
                Order = (short)dayCommand.Order
            };
            await _supabaseClient.From<TrainingDay>().Insert(trainingDay);
        }
        return createdPlan;
    }

    public async Task UpdateWorkoutPlanAsync(WorkoutPlan plan, IEnumerable<UpdateTrainingDayOrderCommand> trainingDays)
    {
        await _supabaseClient
            .From<WorkoutPlan>()
            .Update(plan);

        foreach (var dayUpdate in trainingDays)
        {
            var day = await _supabaseClient
                .From<TrainingDay>()
                .Where(td => td.Id == dayUpdate.Id && td.PlanId == plan.Id)
                .Single();

            if (day != null)
            {
                day.Order = (short)dayUpdate.Order;
                await _supabaseClient.From<TrainingDay>().Update(day);
            }
        }
    }

    public async Task DeleteWorkoutPlanAsync(long planId)
    {
        await _supabaseClient
            .From<WorkoutPlan>()
            .Where(wp => wp.Id == planId)
            .Delete();
    }

    public async Task<bool> IsPlanLockedAsync(long planId, Guid userId)
    {
        var activeSessionsResponse = await _supabaseClient
            .From<Session>()
            .Filter("plan_id", Supabase.Postgrest.Constants.Operator.Equals, planId)
            .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
            .Filter<DateTime?>("end_time", Supabase.Postgrest.Constants.Operator.Is, null)
            .Get();
        return activeSessionsResponse.Models.Any();
    }

    public async Task<IEnumerable<TrainingDay>> GetTrainingDaysWithExercisesAsync(long planId)
    {
        var trainingDaysResponse = await _supabaseClient
            .From<TrainingDay>()
            .Where(td => td.PlanId == planId)
            .Order("order", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();

        var trainingDays = trainingDaysResponse.Models;
        var trainingDayIds = trainingDays.Select(td => td.Id).ToList();

        if (!trainingDayIds.Any())
        {
            return trainingDays;
        }

        var planDayExercisesResponse = await _supabaseClient
            .From<PlanDayExercise>()
            .Where(pde => trainingDayIds.Contains(pde.TrainingDayId))
            .Get();
        
        var planDayExercises = planDayExercisesResponse.Models;
        var exerciseIds = planDayExercises.Select(pde => pde.ExerciseId).Distinct().ToList();

        if (!exerciseIds.Any())
        {
            return trainingDays;
        }

        var exercisesResponse = await _supabaseClient
            .From<Exercise>()
            .Where(e => exerciseIds.Contains(e.Id))
            .Get();
        
        var exercises = exercisesResponse.Models.ToDictionary(e => e.Id, e => e);

        foreach (var day in trainingDays)
        {
            day.PlanDayExercises = planDayExercises
                .Where(pde => pde.TrainingDayId == day.Id)
                .OrderBy(pde => pde.Order)
                .ToList();
            
            foreach (var pde in day.PlanDayExercises)
            {
                if (exercises.TryGetValue(pde.ExerciseId, out var exercise))
                {
                    pde.Exercise = exercise;
                }
            }
        }

        return trainingDays;
    }
}
