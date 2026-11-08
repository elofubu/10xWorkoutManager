using Microsoft.EntityFrameworkCore;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data;
using WorkoutManager.Data.Models;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class SessionExerciseRepository : ISessionExerciseRepository
{
    private readonly WorkoutManagerDbContext _db;

    public SessionExerciseRepository(WorkoutManagerDbContext db)
    {
        _db = db;
    }

    public async Task<Session?> GetSessionByIdAndUserIdAsync(long sessionId, Guid userId)
    {
        return await _db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
    }

    public async Task<SessionExercise?> GetSessionExerciseByIdAndSessionIdAsync(long sessionExerciseId, long sessionId)
    {
        return await _db.SessionExercises
            .FirstOrDefaultAsync(se => se.Id == sessionExerciseId && se.SessionId == sessionId);
    }

    public async Task<SessionExercise?> GetSessionExerciseWithSessionAsync(long sessionExerciseId, Guid userId)
    {
        return await _db.SessionExercises
            .Include(se => se.Session)
            .FirstOrDefaultAsync(se => se.Id == sessionExerciseId && se.Session!.UserId == userId);
    }

    public async Task UpdateSessionExerciseAsync(SessionExercise sessionExercise)
    {
        if (_db.Entry(sessionExercise).State == EntityState.Detached)
        {
            _db.Entry(sessionExercise).State = EntityState.Modified;
        }
        await _db.SaveChangesAsync();
    }

    public async Task DeleteSetsForSessionExerciseAsync(long sessionExerciseId)
    {
        await _db.ExerciseSets.Where(es => es.SessionExerciseId == sessionExerciseId).ExecuteDeleteAsync();
    }

    public async Task<IEnumerable<ExerciseSet>> AddSetsToSessionExerciseAsync(long sessionExerciseId, IEnumerable<ExerciseSet> sets)
    {
        var created = sets.ToList();
        foreach (var set in created)
        {
            set.SessionExerciseId = sessionExerciseId;
        }

        _db.ExerciseSets.AddRange(created);
        await _db.SaveChangesAsync();
        return created;
    }
}
