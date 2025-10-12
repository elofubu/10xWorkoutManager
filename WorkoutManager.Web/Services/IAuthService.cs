using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.Web.Services;

public interface IAuthService
{
    Task<bool> RegisterAsync(string email, string password);
    Task<bool> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<bool> ResetPasswordRequestAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
    Task<bool> DeleteAccountAsync(string password);
    Task<UserDto?> GetCurrentUserAsync();
    Task<bool> IsAuthenticatedAsync();
}

