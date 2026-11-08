using Microsoft.EntityFrameworkCore;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data;
using WorkoutManager.Data.Models;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class WorkoutPlanRepository : IWorkoutPlanRepository
{
    private readonly WorkoutManagerDbContext _db;

    public WorkoutPlanRepository(WorkoutManagerDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<WorkoutPlan>> GetWorkoutPlansAsync(Guid userId)
    {
        return await _db.WorkoutPlans
            .AsNoTracking()
            .AsSplitQuery()
            .Include(wp => wp.TrainingDays.OrderBy(td => td.Order)).ThenInclude(td => td.Exercises)
            .Where(wp => wp.UserId == userId)
            .OrderByDescending(wp => wp.CreatedAt)
            .ToListAsync();
    }

    public async Task<WorkoutPlan?> GetWorkoutPlanByIdAsync(long planId, Guid userId)
    {
        return await _db.WorkoutPlans.FirstOrDefaultAsync(wp => wp.Id == planId && wp.UserId == userId);
    }

    public async Task<WorkoutPlan> CreateWorkoutPlanAsync(WorkoutPlan plan, IEnumerable<CreateTrainingDayCommand> trainingDays)
    {
        foreach (var dayCommand in trainingDays)
        {
            plan.TrainingDays.Add(new TrainingDay
            {
                Name = dayCommand.Name,
                Order = (short)dayCommand.Order
            });
        }

        _db.WorkoutPlans.Add(plan);
        await _db.SaveChangesAsync();
        return plan;
    }

    public async Task UpdateWorkoutPlanAsync(WorkoutPlan plan, IEnumerable<UpdateTrainingDayOrderCommand> trainingDays)
    {
        if (_db.Entry(plan).State == EntityState.Detached)
        {
            _db.Entry(plan).State = EntityState.Modified;
        }

        var updates = trainingDays.ToList();
        var ids = updates.Select(u => u.Id).ToList();
        var days = await _db.TrainingDays
            .Where(td => td.PlanId == plan.Id && ids.Contains(td.Id))
            .ToListAsync();

        foreach (var update in updates)
        {
            var day = days.FirstOrDefault(d => d.Id == update.Id);
            if (day != null)
            {
                day.Order = (short)update.Order;
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task DeleteWorkoutPlanAsync(long planId)
    {
        await _db.WorkoutPlans.Where(wp => wp.Id == planId).ExecuteDeleteAsync();
    }

    public async Task<bool> IsPlanLockedAsync(long planId, Guid userId)
    {
        return await _db.Sessions.AnyAsync(s => s.PlanId == planId && s.UserId == userId && s.EndTime == null);
    }

    public async Task<IEnumerable<TrainingDay>> GetTrainingDaysWithExercisesAsync(long planId)
    {
        return await _db.TrainingDays
            .AsNoTracking()
            .AsSplitQuery()
            .Include(td => td.PlanDayExercises.OrderBy(pde => pde.Order)).ThenInclude(pde => pde.Exercise)
            .Where(td => td.PlanId == planId)
            .OrderBy(td => td.Order)
            .ToListAsync();
    }
}
