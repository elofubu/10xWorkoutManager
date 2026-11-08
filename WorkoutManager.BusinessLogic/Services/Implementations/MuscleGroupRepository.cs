using Microsoft.EntityFrameworkCore;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data;
using WorkoutManager.Data.Models;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class MuscleGroupRepository : IMuscleGroupRepository
{
    private readonly WorkoutManagerDbContext _db;

    public MuscleGroupRepository(WorkoutManagerDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<MuscleGroup>> GetAllAsync()
    {
        return await _db.MuscleGroups.AsNoTracking().OrderBy(mg => mg.Id).ToListAsync();
    }

    public async Task<MuscleGroup?> GetByIdAsync(long id)
    {
        return await _db.MuscleGroups.AsNoTracking().FirstOrDefaultAsync(mg => mg.Id == id);
    }
}
