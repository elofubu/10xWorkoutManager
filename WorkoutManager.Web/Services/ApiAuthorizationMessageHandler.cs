using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace WorkoutManager.Web.Services;

/// <summary>
/// Attaches the Entra External ID access token (acquired and silently refreshed by MSAL) as a
/// Bearer token on requests to WorkoutManager.Api, so its [Authorize] + JWT bearer validation
/// can identify the caller.
/// </summary>
public class ApiAuthorizationMessageHandler : AuthorizationMessageHandler
{
    public ApiAuthorizationMessageHandler(
        IAccessTokenProvider provider,
        NavigationManager navigation,
        IConfiguration configuration)
        : base(provider, navigation)
    {
        var apiUrl = configuration["ApiUrl"] ?? navigation.BaseUri;
        var apiScope = configuration["EntraExternalId:ApiScope"];

        ConfigureHandler(
            authorizedUrls: new[] { apiUrl },
            scopes: string.IsNullOrWhiteSpace(apiScope) ? null : new[] { apiScope });
    }
}
