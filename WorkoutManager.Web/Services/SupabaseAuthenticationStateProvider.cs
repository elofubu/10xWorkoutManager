using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Supabase.Gotrue;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using static Supabase.Gotrue.Constants;

namespace WorkoutManager.Web.Services
{
    public class SupabaseAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly Supabase.Client _supabaseClient;
        private readonly NavigationManager _navigationManager;

        public SupabaseAuthenticationStateProvider(ILocalStorageService localStorage, Supabase.Client supabaseClient, NavigationManager navigationManager)
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
                        _ = _localStorage.SetItemAsync("supabase_session", session);
                    }
                }
                else if (state == AuthState.SignedOut)
                {
                    _ = _localStorage.RemoveItemAsync("supabase_session");
                }

                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            });
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var session = await _localStorage.GetItemAsync<Session>("supabase_session");

                if (session == null)
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                if (session.AccessToken == null || session.ExpiresAt() <= DateTime.UtcNow)
                {
                    await _localStorage.RemoveItemAsync("supabase_session");
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
