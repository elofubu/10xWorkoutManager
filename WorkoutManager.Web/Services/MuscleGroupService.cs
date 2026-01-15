using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using WorkoutManager.BusinessLogic.DTOs;
using BizLogic = WorkoutManager.BusinessLogic.Services.Interfaces;

namespace WorkoutManager.Web.Services;

public class MuscleGroupService : IMuscleGroupService
{
    private readonly BizLogic.IMuscleGroupService _muscleGroupService;
    private readonly AuthenticationStateProvider _authStateProvider;

    public MuscleGroupService(
        BizLogic.IMuscleGroupService muscleGroupService,
        AuthenticationStateProvider authStateProvider)
    {
        _muscleGroupService = muscleGroupService;
        _authStateProvider = authStateProvider;
    }

    private async Task<Guid> GetUserIdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userIdClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User not authenticated"));
    }

    public async Task<IEnumerable<MuscleGroupDto>> GetMuscleGroupsAsync()
    {
        return await _muscleGroupService.GetAllMuscleGroupsAsync();
    }
}
