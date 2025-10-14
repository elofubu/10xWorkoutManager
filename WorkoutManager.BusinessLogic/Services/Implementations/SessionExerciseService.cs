using Supabase;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Exceptions;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class SessionExerciseService : ISessionExerciseService
{
    private readonly Client _supabaseClient;

    public SessionExerciseService(Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<SessionExerciseDetailsDto> UpdateSessionExerciseAsync(
        int sessionId,
        int sessionExerciseId,
        UpdateSessionExerciseCommand command,
        Guid userId)
    {
        // Verify the session exists and belongs to the user
        var session = await _supabaseClient
            .From<Session>()
            .Where(s => s.Id == sessionId)
            .Where(s => s.UserId == userId)
            .Single();

        if (session == null)
        {
            throw new NotFoundException("Session", sessionId);
        }

        // Verify the session exercise exists and belongs to this session
        var sessionExercise = await _supabaseClient
            .From<SessionExercise>()
            .Where(se => se.Id == sessionExerciseId)
            .Where(se => se.SessionId == sessionId)
            .Single();

        if (sessionExercise == null)
        {
            throw new NotFoundException("SessionExercise", sessionExerciseId);
        }

        // Update session exercise properties
        sessionExercise.Notes = command.Notes;
        sessionExercise.Skipped = command.Skipped;

        await _supabaseClient
            .From<SessionExercise>()
            .Update(sessionExercise);

        // Delete existing sets for this session exercise
        await _supabaseClient
            .From<ExerciseSet>()
            .Where(es => es.SessionExerciseId == sessionExerciseId)
            .Delete();

        // Insert new sets if exercise is not skipped
        var sets = new List<ExerciseSetDto>();

        if (!command.Skipped && command.Sets.Any())
        {
            foreach (var setDto in command.Sets)
            {
                var exerciseSet = new ExerciseSet
                {
                    SessionExerciseId = sessionExerciseId,
                    Weight = setDto.Weight,
                    Reps = (short)setDto.Reps,
                    IsFailure = setDto.IsFailure,
                    Order = (short)setDto.Order
                };

                var setResponse = await _supabaseClient
                    .From<ExerciseSet>()
                    .Insert(exerciseSet);

                var createdSet = setResponse.Models.First();

                sets.Add(new ExerciseSetDto
                {
                    Id = (int)createdSet.Id,
                    Weight = createdSet.Weight,
                    Reps = createdSet.Reps,
                    IsFailure = createdSet.IsFailure,
                    Order = createdSet.Order
                });
            }
        }

        return new SessionExerciseDetailsDto
        {
            Id = (int)sessionExercise.Id,
            ExerciseId = (int)sessionExercise.ExerciseId,
            Notes = sessionExercise.Notes,
            Skipped = sessionExercise.Skipped,
            Order = sessionExercise.Order,
            Sets = sets
        };
    }

    public async Task MarkAsSkippedAsync(int sessionExerciseId, Guid userId)
    {
        // Get the session exercise
        var sessionExercise = await _supabaseClient
            .From<SessionExercise>()
            .Where(se => se.Id == sessionExerciseId)
            .Single();

        if (sessionExercise == null)
        {
            throw new NotFoundException("SessionExercise", sessionExerciseId);
        }

        // Verify the session belongs to the user
        var session = await _supabaseClient
            .From<Session>()
            .Where(s => s.Id == sessionExercise.SessionId)
            .Where(s => s.UserId == userId)
            .Single();

        if (session == null)
        {
            throw new Exceptions.UnauthorizedAccessException("You do not have permission to modify this session exercise.");
        }

        // Mark as skipped
        sessionExercise.Skipped = true;

        await _supabaseClient
            .From<SessionExercise>()
            .Update(sessionExercise);

        // Delete any existing sets
        await _supabaseClient
            .From<ExerciseSet>()
            .Where(es => es.SessionExerciseId == sessionExerciseId)
            .Delete();
    }
}

