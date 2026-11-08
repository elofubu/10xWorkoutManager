using Microsoft.EntityFrameworkCore;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data;
using WorkoutManager.Data.Models;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class ExerciseRepository : IExerciseRepository
{
    private readonly WorkoutManagerDbContext _db;

    public ExerciseRepository(WorkoutManagerDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Exercise>> GetExercisesForUserAsync(Guid userId)
    {
        // Predefined exercises (UserId == null) first, then the user's own, matching the old behavior.
        return await _db.Exercises
            .AsNoTracking()
            .Where(e => e.UserId == null || e.UserId == userId)
            .OrderBy(e => e.UserId != null)
            .ThenBy(e => e.Id)
            .ToListAsync();
    }

    public async Task<Exercise?> GetExerciseByIdAsync(long exerciseId)
    {
        return await _db.Exercises.FirstOrDefaultAsync(e => e.Id == exerciseId);
    }

    public async Task<Exercise?> GetExerciseByNameForUserAsync(string name, Guid userId)
    {
        var lowered = name.ToLower();
        return await _db.Exercises
            .AsNoTracking()
            .Where(e => e.UserId == null || e.UserId == userId)
            .FirstOrDefaultAsync(e => e.Name.ToLower() == lowered);
    }

    public async Task<Exercise> CreateExerciseAsync(Exercise exercise)
    {
        _db.Exercises.Add(exercise);
        await _db.SaveChangesAsync();
        return exercise;
    }

    public async Task<PreviousExercisePerformanceDto?> GetLastPerformanceAsync(long exerciseId, Guid userId, long trainingDayId)
    {
        var session = await _db.Sessions
            .AsNoTracking()
            .Where(s => s.UserId == userId
                        && s.TrainingDayId == trainingDayId
                        && s.EndTime != null
                        && s.SessionExercises.Any(se => se.ExerciseId == exerciseId))
            .OrderByDescending(s => s.StartTime)
            .Select(s => new
            {
                s.StartTime,
                s.Notes,
                Sets = s.SessionExercises
                    .Where(se => se.ExerciseId == exerciseId)
                    .SelectMany(se => se.Sets)
                    .OrderBy(set => set.Order)
                    .Select(set => new PreviousExerciseSetDto
                    {
                        Weight = set.Weight,
                        Reps = set.Reps,
                        IsFailure = set.IsFailure
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (session == null) return null;

        return new PreviousExercisePerformanceDto
        {
            SessionDate = session.StartTime,
            Notes = session.Notes,
            Sets = session.Sets
        };
    }
}
