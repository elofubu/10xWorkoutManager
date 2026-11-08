using Microsoft.EntityFrameworkCore;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data;
using WorkoutManager.Data.Models;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class SessionRepository : ISessionRepository
{
    private readonly WorkoutManagerDbContext _db;

    public SessionRepository(WorkoutManagerDbContext db)
    {
        _db = db;
    }

    public async Task<bool> HasActiveSessionAsync(Guid userId)
    {
        return await _db.Sessions.AnyAsync(s => s.UserId == userId && s.EndTime == null);
    }

    public async Task<TrainingDay?> GetTrainingDayByIdAsync(long trainingDayId)
    {
        return await _db.TrainingDays.FirstOrDefaultAsync(td => td.Id == trainingDayId);
    }

    public async Task<WorkoutPlan?> GetWorkoutPlanByIdAsync(long planId, Guid userId)
    {
        return await _db.WorkoutPlans.FirstOrDefaultAsync(wp => wp.Id == planId && wp.UserId == userId);
    }

    public async Task<Session> CreateSessionAsync(Session session)
    {
        _db.Sessions.Add(session);
        await _db.SaveChangesAsync();
        return session;
    }

    public async Task<IEnumerable<PlanDayExercise>> GetPlanDayExercisesAsync(long trainingDayId)
    {
        return await _db.PlanDayExercises
            .AsNoTracking()
            .Where(pde => pde.TrainingDayId == trainingDayId)
            .OrderBy(pde => pde.Order)
            .ToListAsync();
    }

    public async Task<SessionExercise> CreateSessionExerciseAsync(SessionExercise sessionExercise)
    {
        _db.SessionExercises.Add(sessionExercise);
        await _db.SaveChangesAsync();
        return sessionExercise;
    }

    public async Task<IEnumerable<Session>> GetSessionHistoryAsync(Guid userId, int page, int pageSize)
    {
        return await _db.Sessions
            .AsNoTracking()
            .AsSplitQuery()
            .Include(s => s.Plan)
            .Include(s => s.TrainingDay)
            .Include(s => s.SessionExercises.OrderBy(se => se.Order)).ThenInclude(se => se.Exercise)
            .Include(s => s.SessionExercises.OrderBy(se => se.Order)).ThenInclude(se => se.Sets.OrderBy(set => set.Order))
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Session?> GetSessionByIdAsync(long sessionId, Guid userId)
    {
        return await _db.Sessions
            .Include(s => s.Plan)
            .Include(s => s.TrainingDay)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
    }

    public async Task<IEnumerable<SessionExercise>> GetSessionExercisesWithSetsAsync(long sessionId)
    {
        return await _db.SessionExercises
            .AsSplitQuery()
            .Include(se => se.Exercise)
            .Include(se => se.Sets.OrderBy(set => set.Order))
            .Where(se => se.SessionId == sessionId)
            .OrderBy(se => se.Order)
            .ToListAsync();
    }

    public async Task UpdateSessionAsync(Session session)
    {
        if (_db.Entry(session).State == EntityState.Detached)
        {
            _db.Entry(session).State = EntityState.Modified;
        }
        await _db.SaveChangesAsync();
    }

    public async Task<Session?> GetActiveSessionAsync(Guid userId)
    {
        return await _db.Sessions
            .Where(s => s.UserId == userId && s.EndTime == null)
            .OrderByDescending(s => s.StartTime)
            .FirstOrDefaultAsync();
    }

    public async Task DeleteSessionAsync(long sessionId)
    {
        await _db.Sessions.Where(s => s.Id == sessionId).ExecuteDeleteAsync();
    }
}
