using System.Net.Http.Json;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.Web.Services;

public class MuscleGroupService : IMuscleGroupService
{
    private readonly HttpClient _httpClient;

    public MuscleGroupService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<MuscleGroupDto>> GetMuscleGroupsAsync()
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<MuscleGroupDto>>("api/musclegroups") 
            ?? Enumerable.Empty<MuscleGroupDto>();
    }
}

