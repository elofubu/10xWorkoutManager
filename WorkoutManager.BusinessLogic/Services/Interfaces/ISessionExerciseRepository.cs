using WorkoutManager.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface ISessionExerciseRepository
{
    Task<Session?> GetSessionByIdAndUserIdAsync(long sessionId, Guid userId);
    Task<SessionExercise?> GetSessionExerciseByIdAndSessionIdAsync(long sessionExerciseId, long sessionId);
    Task<SessionExercise?> GetSessionExerciseWithSessionAsync(long sessionExerciseId, Guid userId);
    Task UpdateSessionExerciseAsync(SessionExercise sessionExercise);
    Task DeleteSetsForSessionExerciseAsync(long sessionExerciseId);
    Task<IEnumerable<ExerciseSet>> AddSetsToSessionExerciseAsync(long sessionExerciseId, IEnumerable<ExerciseSet> sets);
}


