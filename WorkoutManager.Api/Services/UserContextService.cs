using System.Security.Claims;

namespace WorkoutManager.Api.Services;

public class UserContextService : IUserContextService
{
    // Entra External ID puts the stable per-user identifier in "oid" (object ID).
    private const string ObjectIdClaim = "oid";
    private const string ObjectIdClaimLegacy = "http://schemas.microsoft.com/identity/claims/objectidentifier";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var userId = user?.FindFirst(ObjectIdClaim)?.Value
                     ?? user?.FindFirst(ObjectIdClaimLegacy)?.Value;

        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var parsed))
        {
            throw new InvalidOperationException("User ID (oid claim) not found in token.");
        }

        return parsed;
    }

    public string? GetCurrentUserEmail()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirst("email")?.Value
               ?? user?.FindFirst(ClaimTypes.Email)?.Value
               ?? user?.FindFirst("preferred_username")?.Value;
    }
}
