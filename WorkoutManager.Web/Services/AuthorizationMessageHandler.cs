using System.Net.Http.Headers;
using Blazored.LocalStorage;
using Supabase.Gotrue;

namespace WorkoutManager.Web.Services;

/// <summary>
/// Attaches the current Supabase access token (if any) as a Bearer token on every request
/// made through the "ApiHttpClient" HttpClient, so WorkoutManager.Api's [Authorize] +
/// JWT bearer validation can identify the caller.
/// </summary>
public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public AuthorizationMessageHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Mirrors SupabaseAuthenticationStateProvider's storage key/shape.
        var session = await _localStorage.GetItemAsync<Session>("supabase_session", cancellationToken);
        if (!string.IsNullOrEmpty(session?.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
