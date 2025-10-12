using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.Web.Services;

public class AuthService : IAuthService
{
    private readonly ILocalStorageService _localStorage;
    private readonly NavigationManager _navigationManager;

    public AuthService(
        ILocalStorageService localStorage,
        NavigationManager navigationManager)
    {
        _localStorage = localStorage;
        _navigationManager = navigationManager;
    }

    public async Task<bool> RegisterAsync(string email, string password)
    {
        try
        {
            // TODO: Implement Supabase registration
            // Placeholder for now - will be implemented with Supabase SDK
            return await Task.FromResult(false);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            // TODO: Implement Supabase login
            // Placeholder for now - will be implemented with Supabase SDK
            return await Task.FromResult(false);
        }
        catch
        {
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("accessToken");
        await _localStorage.RemoveItemAsync("refreshToken");
        _navigationManager.NavigateTo("/login");
    }

    public async Task<bool> ResetPasswordRequestAsync(string email)
    {
        try
        {
            // TODO: Implement Supabase password reset
            return await Task.FromResult(false);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        try
        {
            // TODO: Implement Supabase password reset confirmation
            return await Task.FromResult(false);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteAccountAsync(string password)
    {
        try
        {
            // TODO: Implement account deletion
            await _localStorage.RemoveItemAsync("accessToken");
            await _localStorage.RemoveItemAsync("refreshToken");
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("accessToken");
        if (string.IsNullOrEmpty(token))
            return null;

        // TODO: Decode token and extract user info
        return null;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("accessToken");
        return !string.IsNullOrEmpty(token);
    }
}

