using WorkoutManager.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface ISessionExerciseRepository
{
    Task<Session?> GetSessionByIdAndUserIdAsync(int sessionId, Guid userId);
    Task<SessionExercise?> GetSessionExerciseByIdAndSessionIdAsync(int sessionExerciseId, int sessionId);
    Task<SessionExercise?> GetSessionExerciseWithSessionAsync(int sessionExerciseId, Guid userId);
    Task UpdateSessionExerciseAsync(SessionExercise sessionExercise);
    Task DeleteSetsForSessionExerciseAsync(int sessionExerciseId);
    Task<IEnumerable<ExerciseSet>> AddSetsToSessionExerciseAsync(int sessionExerciseId, IEnumerable<ExerciseSet> sets);
}


