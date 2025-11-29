using Supabase;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;

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
            .Where(s => s.UserId == userId)
            .Filter<DateTime?>(s => s.EndTime, Supabase.Postgrest.Constants.Operator.Equals, null)
            .Get();

        return response.Models.Any();
    }

    public async Task<TrainingDay?> GetTrainingDayByIdAsync(long trainingDayId)
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

    public async Task<IEnumerable<PlanDayExercise>> GetPlanDayExercisesAsync(long trainingDayId)
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

        // Fetch sessions
        var response = await _supabaseClient
            .From<Session>()
            .Where(s => s.UserId == userId)
            .Order("start_time", Supabase.Postgrest.Constants.Ordering.Descending)
            .Range(from, to)
            .Get();

        // Fetch related plans
        var planIds = response.Models.Where(s => s.PlanId.HasValue).Select(s => s.PlanId!.Value).Distinct().ToList();
        if (planIds.Any())
        {
            var plansResponse = await _supabaseClient
                .From<WorkoutPlan>()
                .Filter("id", Supabase.Postgrest.Constants.Operator.In, planIds)
                .Get();

            foreach (var model in response.Models)
            {
                var plan = plansResponse.Models.FirstOrDefault(pr => pr.Id == model.PlanId);

                model.Plan = plan;
                model.TrainingDay = plan.TrainingDays.FirstOrDefault(td => td.Id == model.TrainingDayId);
            }
        }

        // Fetch related training days
        //var trainingDayIds = response.Models.Where(s => s.TrainingDayId.HasValue).Select(s => s.TrainingDayId!.Value).Distinct().ToList();
        //if (trainingDayIds.Any())
        //{
        //    var trainingDaysResponse = await _supabaseClient
        //        .From<TrainingDay>()
        //        .Filter("id", Supabase.Postgrest.Constants.Operator.In, trainingDayIds)
        //        .Get();

        //    foreach (var model in response.Models)
        //    {
        //        model.TrainingDay = trainingDaysResponse.Models.FirstOrDefault(td => td.Id == model.TrainingDayId);
        //    }
        //}

        return response.Models;
    }

    public async Task<Session?> GetSessionByIdAsync(long sessionId, Guid userId)
    {
        var session = await _supabaseClient
            .From<Session>()
            .Where(s => s.Id == sessionId && s.UserId == userId)
            .Single();

        if (session != null)
        {
            // Fetch related plan if exists
            if (session.PlanId.HasValue)
            {
                session.Plan = await _supabaseClient
                    .From<WorkoutPlan>()
                    .Where(wp => wp.Id == session.PlanId.Value)
                    .Single();
            }

            // Fetch related training day if exists
            if (session.TrainingDayId.HasValue)
            {
                session.TrainingDay = await _supabaseClient
                    .From<TrainingDay>()
                    .Where(td => td.Id == session.TrainingDayId.Value)
                    .Single();
            }
        }

        return session;
    }

    public async Task<IEnumerable<SessionExercise>> GetSessionExercisesWithSetsAsync(long sessionId)
    {
        var sessionExercises = await _supabaseClient
            .From<SessionExercise>()
            .Where(se => se.SessionId == sessionId)
            .Order("order", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();

        //return response.Models;
        var exerciseIds = sessionExercises.Models.Select(se => se.ExerciseId).ToList();

        if (exerciseIds.Any() == false)
            return sessionExercises.Models;

        var exerciseResponse = await _supabaseClient
            .From<Exercise>()
            .Filter("id", Supabase.Postgrest.Constants.Operator.In, exerciseIds)
            .Get();

        if (exerciseResponse.Models.Any() == false)
            return sessionExercises.Models;

        foreach (var sessionExercise in sessionExercises.Models)
        {
            var exercise = exerciseResponse.Models.FirstOrDefault(e => e.Id == sessionExercise.ExerciseId);
            sessionExercise.Exercise = exercise;
        }

        return sessionExercises.Models;

        //if (sessionExerciseIds.Any())
        //{
        //    var setsResponse = await _supabaseClient
        //        .From<ExerciseSet>()
        //        .Filter(es => es.SessionExerciseId, Supabase.Postgrest.Constants.Operator.In, sessionExerciseIds)
        //        .Get();

        //    var sets = setsResponse.Models.GroupBy(es => es.SessionExerciseId).ToDictionary(g => g.Key, g => g.ToList());

        //    foreach (var se in sessionExercises)
        //    {
        //        if (sets.TryGetValue(se.Id, out var exerciseSets))
        //        {
        //            se.Sets = exerciseSets.OrderBy(s => s.Order).ToList();
        //        }
        //    }
        //}
        //return sessionExercises;
    }

    public async Task UpdateSessionAsync(Session session)
    {
        await _supabaseClient.From<Session>().Update(session);
    }

    public async Task<Session?> GetActiveSessionAsync(Guid userId)
    {
        var response = await _supabaseClient
            .From<Session>()
            .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
            .Filter<DateTime?>(s => s.EndTime, Supabase.Postgrest.Constants.Operator.Equals, null)
            .Limit(1)
            .Get();

        return response.Models.FirstOrDefault();
    }
}
