using Supabase;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class SessionExerciseRepository : ISessionExerciseRepository
{
    private readonly Client _supabaseClient;

    public SessionExerciseRepository(Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<Session?> GetSessionByIdAndUserIdAsync(long sessionId, Guid userId)
    {
        return await _supabaseClient
            .From<Session>()
            .Where(s => s.Id == sessionId && s.UserId == userId)
            .Single();
    }

    public async Task<SessionExercise?> GetSessionExerciseByIdAndSessionIdAsync(long sessionExerciseId, long sessionId)
    {
        return await _supabaseClient
            .From<SessionExercise>()
            .Where(se => se.Id == sessionExerciseId && se.SessionId == sessionId)
            .Single();
    }

    public async Task<SessionExercise?> GetSessionExerciseWithSessionAsync(long sessionExerciseId, Guid userId)
    {
        var sessionExercise = await _supabaseClient
            .From<SessionExercise>()
            .Where(se => se.Id == sessionExerciseId)
            .Single();

        if(sessionExercise == null) return null;

        var session = await GetSessionByIdAndUserIdAsync(sessionExercise.SessionId, userId);
        if(session == null) return null;

        sessionExercise.Session = session;
        return sessionExercise;
    }

    public async Task UpdateSessionExerciseAsync(SessionExercise sessionExercise)
    {
        await _supabaseClient
            .From<SessionExercise>()
            .Update(sessionExercise);
    }

    public async Task DeleteSetsForSessionExerciseAsync(long sessionExerciseId)
    {
        await _supabaseClient
            .From<ExerciseSet>()
            .Where(es => es.SessionExerciseId == sessionExerciseId)
            .Delete();
    }

    public async Task<IEnumerable<ExerciseSet>> AddSetsToSessionExerciseAsync(long sessionExerciseId, IEnumerable<ExerciseSet> sets)
    {
        var createdSets = new List<ExerciseSet>();
        foreach (var exerciseSet in sets)
        {
            var response = await _supabaseClient
                .From<ExerciseSet>()
                .Insert(exerciseSet);
            createdSets.Add(response.Models[0]);
        }
        return createdSets;
    }
}
