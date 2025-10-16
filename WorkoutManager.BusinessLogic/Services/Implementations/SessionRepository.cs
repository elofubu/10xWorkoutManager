using Supabase;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class SessionRepository : ISessionRepository
{
    private readonly Client _supabaseClient;

    public SessionRepository(Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<bool> HasActiveSessionAsync(Guid userId)
    {
        var response = await _supabaseClient
            .From<Session>()
            .Where(s => s.UserId == userId && s.EndTime == null)
            .Get();
        return response.Models.Any();
    }

    public async Task<TrainingDay?> GetTrainingDayByIdAsync(int trainingDayId)
    {
        return await _supabaseClient
            .From<TrainingDay>()
            .Where(td => td.Id == trainingDayId)
            .Single();
    }

    public async Task<WorkoutPlan?> GetWorkoutPlanByIdAsync(long planId, Guid userId)
    {
        return await _supabaseClient
            .From<WorkoutPlan>()
            .Where(wp => wp.Id == planId && wp.UserId == userId)
            .Single();
    }

    public async Task<Session> CreateSessionAsync(Session session)
    {
        var response = await _supabaseClient.From<Session>().Insert(session);
        return response.Models.First();
    }

    public async Task<IEnumerable<PlanDayExercise>> GetPlanDayExercisesAsync(int trainingDayId)
    {
        var response = await _supabaseClient
            .From<PlanDayExercise>()
            .Where(pde => pde.TrainingDayId == trainingDayId)
            .Order("order", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();
        return response.Models;
    }

    public async Task<SessionExercise> CreateSessionExerciseAsync(SessionExercise sessionExercise)
    {
        var response = await _supabaseClient.From<SessionExercise>().Insert(sessionExercise);
        return response.Models.First();
    }

    public async Task<IEnumerable<Session>> GetSessionHistoryAsync(Guid userId, int page, int pageSize)
    {
        var from = (page - 1) * pageSize;
        var to = from + pageSize - 1;

        var response = await _supabaseClient
            .From<Session>()
            .Where(s => s.UserId == userId)
            .Order("start_time", Supabase.Postgrest.Constants.Ordering.Descending)
            .Range(from, to)
            .Get();
        
        var sessions = response.Models;
        var planIds = sessions.Where(s => s.PlanId.HasValue).Select(s => s.PlanId.Value).Distinct().ToList();

        if (planIds.Any())
        {
            var plansResponse = await _supabaseClient
                .From<WorkoutPlan>()
                .Where(wp => planIds.Contains(wp.Id))
                .Get();
            var plans = plansResponse.Models.ToDictionary(p => p.Id);

            foreach (var session in sessions)
            {
                if (session.PlanId.HasValue && plans.TryGetValue(session.PlanId.Value, out var plan))
                {
                    session.Plan = plan;
                }
            }
        }
        return sessions;
    }

    public async Task<Session?> GetSessionByIdAsync(int sessionId, Guid userId)
    {
        return await _supabaseClient
            .From<Session>()
            .Where(s => s.Id == sessionId && s.UserId == userId)
            .Single();
    }

    public async Task<IEnumerable<SessionExercise>> GetSessionExercisesWithSetsAsync(int sessionId)
    {
        var response = await _supabaseClient
            .From<SessionExercise>()
            .Where(se => se.SessionId == sessionId)
            .Order("order", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();
        
        var sessionExercises = response.Models;
        var sessionExerciseIds = sessionExercises.Select(se => se.Id).ToList();

        if (sessionExerciseIds.Any())
        {
            var setsResponse = await _supabaseClient
                .From<ExerciseSet>()
                .Where(es => sessionExerciseIds.Contains(es.SessionExerciseId))
                .Get();
            var sets = setsResponse.Models.GroupBy(es => es.SessionExerciseId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var se in sessionExercises)
            {
                if (sets.TryGetValue(se.Id, out var exerciseSets))
                {
                    se.Sets = exerciseSets.OrderBy(s => s.Order).ToList();
                }
            }
        }
        return sessionExercises;
    }
    
    public async Task UpdateSessionAsync(Session session)
    {
        await _supabaseClient.From<Session>().Update(session);
    }

    public async Task<Session?> GetActiveSessionAsync(Guid userId)
    {
        var response = await _supabaseClient
            .From<Session>()
            .Where(s => s.UserId == userId && s.EndTime == null)
            .Limit(1)
            .Get();
        return response.Models.FirstOrDefault();
    }
}
