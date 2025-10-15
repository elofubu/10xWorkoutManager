using Supabase;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Exceptions;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class ExerciseService : IExerciseService
{
    private readonly Client _supabaseClient;

    public ExerciseService(Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<PaginatedList<ExerciseDto>> GetExercisesAsync(
        Guid userId,
        string? search = null,
        int? muscleGroupId = null,
        int page = 1,
        int pageSize = 20)
    {
        var from = (page - 1) * pageSize;
        var to = from + pageSize - 1;

        // Build query - get all exercises first, then filter in memory
        // This is necessary because RLS policies handle user visibility
        var response = await _supabaseClient
            .From<Exercise>()
            .Range(from, to)
            .Get();

        // Filter in-memory for user visibility (predefined or owned)
        var filteredExercises = response.Models
            .Where(e => e.UserId == null || e.UserId == userId);

        // Apply muscle group filter if provided
        if (muscleGroupId.HasValue)
        {
            filteredExercises = filteredExercises.Where(e => e.MuscleGroupId == muscleGroupId.Value);
        }

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            filteredExercises = filteredExercises.Where(e => 
                e.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var exercisesList = filteredExercises.ToList();

        var dtos = exercisesList.Select(e => new ExerciseDto
        {
            Id = (int)e.Id,
            UserId = e.UserId,
            MuscleGroupId = (int)e.MuscleGroupId,
            Name = e.Name
        }).ToList();

        return new PaginatedList<ExerciseDto>
        {
            Data = dtos,
            Pagination = new PaginationInfo
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = exercisesList.Count
            }
        };
    }

    public async Task<ExerciseDto?> GetExerciseByIdAsync(int exerciseId)
    {
        var response = await _supabaseClient
            .From<Exercise>()
            .Where(e => e.Id == exerciseId)
            .Single();

        if (response == null) return null;

        return new ExerciseDto
        {
            Id = (int)response.Id,
            UserId = response.UserId,
            MuscleGroupId = (int)response.MuscleGroupId,
            Name = response.Name
        };
    }

    public async Task<ExerciseDto> CreateExerciseAsync(CreateExerciseDto dto, Guid userId)
    {
        // Check for duplicate name for this user
        var existing = await _supabaseClient
            .From<Exercise>()
            .Where(e => e.UserId == userId)
            .Where(e => e.Name.ToLower() == dto.Name.ToLower())
            .Get();

        if (existing.Models.Any())
        {
            throw new BusinessRuleViolationException($"An exercise named '{dto.Name}' already exists.");
        }

        var exercise = new Exercise
        {
            UserId = userId,
            MuscleGroupId = dto.MuscleGroupId,
            Name = dto.Name
        };

        var response = await _supabaseClient
            .From<Exercise>()
            .Insert(exercise);

        var created = response.Models.First();

        return new ExerciseDto
        {
            Id = (int)created.Id,
            UserId = created.UserId,
            MuscleGroupId = (int)created.MuscleGroupId,
            Name = created.Name
        };
    }

    public async Task<PreviousExercisePerformanceDto?> GetLastPerformanceAsync(int exerciseId, Guid userId)
    {
        // Get session exercises for this exercise and user (not skipped)
        // Order by session start time descending to get the most recent
        var sessionExercisesResponse = await _supabaseClient
            .From<SessionExercise>()
            .Where(se => se.ExerciseId == exerciseId)
            .Where(se => se.Skipped == false)
            .Get();

        if (!sessionExercisesResponse.Models.Any()) return null;

        // For each session exercise, get the corresponding session to check user and get date
        SessionExercise? mostRecentSessionExercise = null;
        Session? mostRecentSession = null;

        foreach (var se in sessionExercisesResponse.Models)
        {
            var session = await _supabaseClient
                .From<Session>()
                .Where(s => s.Id == se.SessionId)
                .Where(s => s.UserId == userId)
                .Where(s => s.EndTime != null) // Only completed sessions
                .Single();

            if (session != null)
            {
                if (mostRecentSession == null || session.StartTime > mostRecentSession.StartTime)
                {
                    mostRecentSession = session;
                    mostRecentSessionExercise = se;
                }
            }
        }

        if (mostRecentSessionExercise == null || mostRecentSession == null)
        {
            return null;
        }

        // Get the sets for this session exercise
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

