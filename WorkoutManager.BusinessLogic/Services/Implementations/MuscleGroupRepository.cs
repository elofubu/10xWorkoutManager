using Supabase;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class MuscleGroupRepository : IMuscleGroupRepository
{
    private readonly Client _supabaseClient;

    public MuscleGroupRepository(Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<IEnumerable<MuscleGroup>> GetAllAsync()
    {
        var response = await _supabaseClient
            .From<MuscleGroup>()
            .Get();

        return response.Models;
    }

    public async Task<MuscleGroup?> GetByIdAsync(long id)
    {
        var response = await _supabaseClient
            .From<MuscleGroup>()
            .Where(mg => mg.Id == id)
            .Single();

        return response;
    }
}
