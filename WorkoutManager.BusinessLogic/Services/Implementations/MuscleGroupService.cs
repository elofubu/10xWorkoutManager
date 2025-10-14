using Supabase;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class MuscleGroupService : IMuscleGroupService
{
    private readonly Client _supabaseClient;

    public MuscleGroupService(Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<PaginatedList<MuscleGroupDto>> GetAllMuscleGroupsAsync(int page = 1, int pageSize = 20)
    {
        var from = (page - 1) * pageSize;
        var to = from + pageSize - 1;

        var response = await _supabaseClient
            .From<MuscleGroup>()
            .Range(from, to)
            .Get();

        // Get total count from response headers or models count
        var count = response.Models.Count;

        var dtos = response.Models.Select(mg => new MuscleGroupDto
        {
            Id = (int)mg.Id,
            Name = mg.Name
        }).ToList();

        return new PaginatedList<MuscleGroupDto>
        {
            Data = dtos,
            Pagination = new PaginationInfo
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = count
            }
        };
    }

    public async Task<MuscleGroupDto?> GetMuscleGroupByIdAsync(int id)
    {
        var response = await _supabaseClient
            .From<MuscleGroup>()
            .Where(mg => mg.Id == id)
            .Single();

        if (response == null) return null;

        return new MuscleGroupDto
        {
            Id = (int)response.Id,
            Name = response.Name
        };
    }
}

