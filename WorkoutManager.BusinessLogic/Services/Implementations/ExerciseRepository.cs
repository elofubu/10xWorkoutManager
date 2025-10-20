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
        // Fix: Use .Where with a predicate instead of .Or with a string
        var generalExercises = await _supabaseClient
            .From<Exercise>()
            .Where(e => e.UserId == null)
            .Get();

        var userExercises = await _supabaseClient
            .From<Exercise>()
            .Where(e => e.UserId == userId)
            .Get();

        return generalExercises.Models.Concat(userExercises.Models);
    }

    public async Task<Exercise?> GetExerciseByIdAsync(long exerciseId)
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
            .Where(e => e.UserId == null || e.UserId == userId)
            .Filter(e => e.Name.ToLower(), Supabase.Postgrest.Constants.Operator.Equals, name.ToLower())
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

    public async Task<PreviousExercisePerformanceDto?> GetLastPerformanceAsync(long exerciseId, Guid userId)
    {
        // Optimized: Single query with nested projections for complete data hierarchy
        // Fetch session exercises with related sessions and exercise sets, sorted and limited at database level
        var sessionExercisesResponse = await _supabaseClient
            .From<SessionExercise>()
            .Select("*, session(*), exercise_sets(*)")
            .Where(se => se.ExerciseId == exerciseId && se.Skipped == false)
            .Order("session.start_time", Supabase.Postgrest.Constants.Ordering.Descending)
            .Limit(1)
            .Get();

        if (!sessionExercisesResponse.Models.Any()) return null;

        var mostRecentSessionExercise = sessionExercisesResponse.Models.First();

        // Verify session is valid (completed by the correct user)
        if (mostRecentSessionExercise.Session == null ||
            mostRecentSessionExercise.Session.UserId != userId ||
            !mostRecentSessionExercise.Session.EndTime.HasValue)
        {
            return null;
        }

        return new PreviousExercisePerformanceDto
        {
            SessionDate = mostRecentSessionExercise.Session.StartTime,
            Notes = mostRecentSessionExercise.Notes,
            Sets = mostRecentSessionExercise.Sets
                ?.OrderBy(s => s.Order)
                .Select(s => new PreviousExerciseSetDto
                {
                    Weight = s.Weight,
                    Reps = s.Reps,
                    IsFailure = s.IsFailure
                }).ToList() ?? new List<PreviousExerciseSetDto>()
        };
    }
}
