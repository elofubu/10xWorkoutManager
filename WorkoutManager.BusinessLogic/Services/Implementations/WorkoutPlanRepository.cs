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
            .Where(wp => wp.Id == planId)
            .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
            .Single();
    }

    public async Task<WorkoutPlan> CreateWorkoutPlanAsync(WorkoutPlan plan, IEnumerable<CreateTrainingDayCommand> trainingDays)
    {
        var planResponse = await _supabaseClient
            .From<WorkoutPlan>()
            .Insert(plan);
        var createdPlan = planResponse.Models.First();

        var tradiningDaysList = new List<TrainingDay>();

        foreach (var dayCommand in trainingDays)
        {
            var trainingDay = new TrainingDay
            {
                PlanId = createdPlan.Id,
                Name = dayCommand.Name,
                Order = (short)dayCommand.Order
            };

            var trainingDayModel = await _supabaseClient.From<TrainingDay>().Insert(trainingDay);

            if (trainingDayModel.Model is not null)
                tradiningDaysList.Add(trainingDayModel.Model);
        }

        //createdPlan.TrainingDays = tradiningDaysList;

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
        // Fetch training days with both Exercise relations and PlanDayExercise (for order preservation)
        var trainingDaysResponse = await _supabaseClient
            .From<TrainingDay>()
            //.Select("*, plan_day_exercises!training_day_id(*)")
            //.Select("*, plan_day_exercises(*, exercises(*))")
            //.Select("*, exercises(*), plan_day_exercises(*, exercises(*))")
            .Filter("plan_id", Supabase.Postgrest.Constants.Operator.Equals, planId)
            .Order("order", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();

        var trainingDays = trainingDaysResponse.Models.ToList();

        if (!trainingDays.Any())
            return trainingDays;

        return trainingDays;
    }
}
