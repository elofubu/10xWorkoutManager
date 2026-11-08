using Microsoft.EntityFrameworkCore;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data;
using WorkoutManager.Data.Models;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class PlanExerciseRepository : IPlanExerciseRepository
{
    private readonly WorkoutManagerDbContext _db;

    public PlanExerciseRepository(WorkoutManagerDbContext db)
    {
        _db = db;
    }

    public async Task<WorkoutPlan?> GetPlanByIdAndUserIdAsync(long planId, Guid userId)
    {
        return await _db.WorkoutPlans.FirstOrDefaultAsync(wp => wp.Id == planId && wp.UserId == userId);
    }

    public async Task<TrainingDay?> GetTrainingDayByIdAndPlanIdAsync(long dayId, long planId)
    {
        return await _db.TrainingDays.FirstOrDefaultAsync(td => td.Id == dayId && td.PlanId == planId);
    }

    public async Task<Exercise?> GetExerciseByIdAsync(long exerciseId)
    {
        return await _db.Exercises.FirstOrDefaultAsync(e => e.Id == exerciseId);
    }

    public async Task<PlanDayExercise> AddExerciseToDayAsync(PlanDayExercise planDayExercise)
    {
        _db.PlanDayExercises.Add(planDayExercise);
        await _db.SaveChangesAsync();
        return planDayExercise;
    }

    public async Task<PlanDayExercise?> GetPlanDayExerciseAsync(long planDayExerciseId, long dayId)
    {
        return await _db.PlanDayExercises
            .FirstOrDefaultAsync(pde => pde.Id == planDayExerciseId && pde.TrainingDayId == dayId);
    }

    public async Task RemoveExerciseFromDayAsync(long planDayExerciseId)
    {
        await _db.PlanDayExercises.Where(pde => pde.Id == planDayExerciseId).ExecuteDeleteAsync();
    }

    public async Task ReorderExercisesAsync(long dayId, List<ReorderExerciseCommand> exercises)
    {
        var ids = exercises.Select(e => e.Id).ToList();
        var rows = await _db.PlanDayExercises
            .Where(pde => pde.TrainingDayId == dayId && ids.Contains(pde.Id))
            .ToListAsync();

        foreach (var update in exercises)
        {
            var row = rows.FirstOrDefault(r => r.Id == update.Id);
            if (row != null)
            {
                row.Order = (short)update.Order;
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<(Exercise Exercise, short Order)>> GetExercisesWithOrderAsync(long trainingDayId)
    {
        var rows = await _db.PlanDayExercises
            .AsNoTracking()
            .Include(pde => pde.Exercise)
            .Where(pde => pde.TrainingDayId == trainingDayId)
            .OrderBy(pde => pde.Order)
            .ToListAsync();

        return rows
            .Where(pde => pde.Exercise != null)
            .Select(pde => (pde.Exercise!, pde.Order))
            .ToList();
    }
}
