namespace WorkoutManager.Api.Services;

public interface IUserContextService
{
    Guid GetCurrentUserId();
    string? GetCurrentUserEmail();
}

