using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Exceptions;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class SessionExerciseService : ISessionExerciseService
{
    private readonly ISessionExerciseRepository _sessionExerciseRepository;

    public SessionExerciseService(ISessionExerciseRepository sessionExerciseRepository)
    {
        _sessionExerciseRepository = sessionExerciseRepository;
    }

    public async Task<SessionExerciseDetailsDto> UpdateSessionExerciseAsync(
        long sessionId,
        long sessionExerciseId,
        UpdateSessionExerciseCommand command,
        Guid userId)
    {
        var session = await _sessionExerciseRepository.GetSessionByIdAndUserIdAsync(sessionId, userId);
        if (session == null)
        {
            throw new NotFoundException("Session", sessionId);
        }

        var sessionExercise = await _sessionExerciseRepository.GetSessionExerciseByIdAndSessionIdAsync(sessionExerciseId, sessionId);
        if (sessionExercise == null)
        {
            throw new NotFoundException("SessionExercise", sessionExerciseId);
        }

        sessionExercise.Notes = command.Notes;
        sessionExercise.Skipped = command.Skipped;
        await _sessionExerciseRepository.UpdateSessionExerciseAsync(sessionExercise);

        await _sessionExerciseRepository.DeleteSetsForSessionExerciseAsync(sessionExerciseId);

        var setsToCreate = command.Sets.Select(s => new ExerciseSet
        {
            SessionExerciseId = sessionExerciseId,
            Weight = s.Weight,
            Reps = (short)s.Reps,
            IsFailure = s.IsFailure,
            Order = (short)s.Order
        });

        var createdSets = await _sessionExerciseRepository.AddSetsToSessionExerciseAsync(sessionExerciseId, setsToCreate);

        var setDtos = createdSets.Select(s => new ExerciseSetDto
        {
            Id = (int)s.Id,
            Weight = s.Weight,
            Reps = s.Reps,
            IsFailure = s.IsFailure,
            Order = s.Order
        }).ToList();

        return new SessionExerciseDetailsDto
        {
            Id = (int)sessionExercise.Id,
            ExerciseId = (int)sessionExercise.ExerciseId,
            Notes = sessionExercise.Notes,
            Skipped = sessionExercise.Skipped,
            Order = sessionExercise.Order,
            Sets = setDtos
        };
    }

    public async Task MarkAsSkippedAsync(long sessionExerciseId, Guid userId)
    {
        var sessionExercise = await _sessionExerciseRepository.GetSessionExerciseWithSessionAsync(sessionExerciseId, userId);
        if (sessionExercise == null)
        {
            throw new NotFoundException("SessionExercise", sessionExerciseId);
        }

        sessionExercise.Skipped = true;
        await _sessionExerciseRepository.UpdateSessionExerciseAsync(sessionExercise);

        await _sessionExerciseRepository.DeleteSetsForSessionExerciseAsync(sessionExerciseId);
    }
}

