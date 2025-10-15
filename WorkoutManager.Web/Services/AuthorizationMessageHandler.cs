using Blazored.LocalStorage;
using Supabase.Gotrue;
using System.Net.Http.Headers;
using System.Text.Json;

namespace WorkoutManager.Web.Services
{
    public class AuthorizationMessageHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;

        public AuthorizationMessageHandler(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var sessionJson = await _localStorage.GetItemAsStringAsync("supabase_session");

            if (!string.IsNullOrEmpty(sessionJson))
            {
                var session = JsonSerializer.Deserialize<Session>(sessionJson);
                if (session?.AccessToken != null && session.ExpiresAt() > DateTime.UtcNow)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}

