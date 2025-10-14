using System.Security.Claims;

namespace WorkoutManager.Api.Services;

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetCurrentUserId()
    {
        //var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        //    ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

        //if (string.IsNullOrEmpty(userIdClaim))
        //    throw new UnauthorizedAccessException("User ID not found in token");

        //return Guid.Parse(userIdClaim);

        return Guid.Parse("f9c4a701-b152-4081-a45a-65cfc95ceac7");
    }

    public string? GetCurrentUserEmail()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
    }
}

