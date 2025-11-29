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
            .Where(e => e.UserId == null)
            .Where(e => e.UserId == userId)
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

    public async Task<PreviousExercisePerformanceDto?> GetLastPerformanceAsync(long exerciseId, Guid userId, long trainingDayId)
    {
        var session = await _supabaseClient
            .From<Session>()
            .Filter("training_day_id", Supabase.Postgrest.Constants.Operator.Equals, trainingDayId)
            .Filter("session_exercises.exercise_id", Supabase.Postgrest.Constants.Operator.Equals, exerciseId)
            .Filter<DateTime?>("end_time", Supabase.Postgrest.Constants.Operator.Not, null)
            .Order("start_time", Supabase.Postgrest.Constants.Ordering.Descending)
            .Get();

        if (!session.Models.Any()) return null;

        var recentSession = session.Models.FirstOrDefault();

        // Verify session is valid (completed by the correct user)
        if (recentSession == null ||
            recentSession.UserId != userId ||
            !recentSession.EndTime.HasValue)
        {
            return null;
        }

        return new PreviousExercisePerformanceDto
        {
            SessionDate = recentSession.StartTime,
            Notes = recentSession.Notes,
            Sets = recentSession.SessionExercises
                .SelectMany(se => se.Sets ?? new List<ExerciseSet>())
                .OrderBy(s => s.Order)
                .Select(s => new PreviousExerciseSetDto
                {
                    Weight = s.Weight,
                    Reps = s.Reps,
                    IsFailure = s.IsFailure
                })
                .ToList()
        };
    }
}
