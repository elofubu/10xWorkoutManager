using Blazored.LocalStorage;
using Supabase;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly Client _supabaseClient;
        private readonly ILocalStorageService _localStorage;

        public AuthService(Client supabaseClient, ILocalStorageService localStorage)
        {
            _supabaseClient = supabaseClient;
            _localStorage = localStorage;
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
            var session = await _supabaseClient.Auth.SignIn(email, password);
            if (session != null)
            {
                await _localStorage.SetItemAsync("supabase_session", session);
                return true;
            }
            return false;
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync("supabase_session");
        }

        public async Task ResetPasswordAsync(string email)
        {
            await _supabaseClient.Auth.ResetPasswordForEmail(email);
        }

        public async Task UpdatePasswordAsync(string newPassword)
        {
            if (_supabaseClient.Auth.CurrentUser == null)
            {
                throw new InvalidOperationException("User must be logged in to update password.");
            }

            await _supabaseClient.Auth.Update(new Supabase.Gotrue.UserAttributes
            {
                Password = newPassword
            });
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

        //public async Task<UserDto?> IAuthService.GetCurrentUserAsync()
        //{
        //    throw new NotImplementedException();
        //}
    }
}

