using Supabase;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class ExerciseRepository : IExerciseRepository
{
    private readonly Client _supabaseClient;

    public ExerciseRepository(Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<IEnumerable<Exercise>> GetExercisesForUserAsync(Guid userId)
    {
        var response = await _supabaseClient
            .From<Exercise>()
            .Get();

        return response.Models.Where(e => e.UserId == null || e.UserId == userId);
    }

    public async Task<Exercise?> GetExerciseByIdAsync(int exerciseId)
    {
        return await _supabaseClient
            .From<Exercise>()
            .Where(e => e.Id == exerciseId)
            .Single();
    }

    public async Task<Exercise?> GetExerciseByNameForUserAsync(string name, Guid userId)
    {
        var response = await _supabaseClient
            .From<Exercise>()
            .Where(e => e.UserId == userId && e.Name.ToLower() == name.ToLower())
            .Get();
        
        return response.Models.FirstOrDefault();
    }

    public async Task<Exercise> CreateExerciseAsync(Exercise exercise)
    {
        var response = await _supabaseClient
            .From<Exercise>()
            .Insert(exercise);

        return response.Models.First();
    }

    public async Task<PreviousExercisePerformanceDto?> GetLastPerformanceAsync(int exerciseId, Guid userId)
    {
        var sessionExercisesResponse = await _supabaseClient
            .From<SessionExercise>()
            .Where(se => se.ExerciseId == exerciseId && se.Skipped == false)
            .Get();

        if (!sessionExercisesResponse.Models.Any()) return null;

        var sessionIds = sessionExercisesResponse.Models.Select(se => se.SessionId).ToList();
        
        var sessionsResponse = await _supabaseClient
            .From<Session>()
            .Where(s => sessionIds.Contains(s.Id))
            .Where(s => s.UserId == userId && s.EndTime != null)
            .Get();
            
        if (!sessionsResponse.Models.Any()) return null;

        var mostRecentSession = sessionsResponse.Models.OrderByDescending(s => s.StartTime).First();
        var mostRecentSessionExercise = sessionExercisesResponse.Models.First(se => se.SessionId == mostRecentSession.Id);

        var setsResponse = await _supabaseClient
            .From<ExerciseSet>()
            .Where(s => s.SessionExerciseId == mostRecentSessionExercise.Id)
            .Order("order", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();

        return new PreviousExercisePerformanceDto
        {
            SessionDate = mostRecentSession.StartTime,
            Notes = mostRecentSessionExercise.Notes,
            Sets = setsResponse.Models.Select(s => new PreviousExerciseSetDto
            {
                Weight = s.Weight,
                Reps = s.Reps,
                IsFailure = s.IsFailure
            }).ToList()
        };
    }
}
