using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Supabase.Gotrue;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using static Supabase.Gotrue.Constants;

namespace WorkoutManager.Web.Services
{
    public class SupabaseAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedLocalStorage _localStorage;
        private readonly Supabase.Client _supabaseClient;
        private readonly NavigationManager _navigationManager;

        public SupabaseAuthenticationStateProvider(ProtectedLocalStorage localStorage, Supabase.Client supabaseClient, NavigationManager navigationManager)
        {
            _localStorage = localStorage;
            _supabaseClient = supabaseClient;
            _navigationManager = navigationManager;

            _supabaseClient.Auth.AddStateChangedListener((sender, state) =>
            {
                if (state == AuthState.PasswordRecovery)
                {
                    _navigationManager.NavigateTo("/authentication/update-password");
                }

                if (state == AuthState.SignedIn)
                {
                    var session = _supabaseClient.Auth.CurrentSession;
                    if (session != null)
                    {
                        _ = _localStorage.SetAsync("supabase_session", session);
                    }
                }
                else if (state == AuthState.SignedOut)
                {
                    _ = _localStorage.DeleteAsync("supabase_session");
                }

                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            });
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var result = await _localStorage.GetAsync<Session>("supabase_session");

                if (!result.Success || result.Value == null)
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var session = result.Value;

                if (session.AccessToken == null || session.ExpiresAt() <= DateTime.UtcNow)
                {
                    await _localStorage.DeleteAsync("supabase_session");
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                //check for expiration
                var jwtHandler = new JwtSecurityTokenHandler();
                var jwtToken = jwtHandler.ReadToken(session.AccessToken);
                if (jwtToken.ValidTo <= DateTime.UtcNow)
                {
                    //refresh the token
                    await _supabaseClient.Auth.RefreshSession();
                }

                var claims = new[] {
                    new Claim(ClaimTypes.NameIdentifier, session.User.Id),
                    new Claim(ClaimTypes.Email, session.User.Email),
                    new Claim(ClaimTypes.Name, session.User.Email)
                };

                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public void NotifyUserAuthenticationStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
