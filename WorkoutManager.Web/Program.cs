using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using WorkoutManager.Web;
using WorkoutManager.Web.Components;
using WorkoutManager.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Browser-native storage (replaces Blazor Server's ProtectedLocalStorage/ProtectedSessionStorage)
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredSessionStorage();

// HTTP client for WorkoutManager.Api — every request gets the Entra access token attached
var apiUrl = builder.Configuration["ApiUrl"] ?? builder.HostEnvironment.BaseAddress;
if (!apiUrl.EndsWith('/')) apiUrl += "/"; // relative request URIs (e.g. "api/exercises") need a trailing slash on the base

builder.Services.AddTransient<ApiAuthorizationMessageHandler>();
builder.Services.AddHttpClient("ApiHttpClient", client => client.BaseAddress = new Uri(apiUrl))
    .AddHttpMessageHandler<ApiAuthorizationMessageHandler>();
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiHttpClient"));

// Authentication
// Microsoft Entra External ID via MSAL (redirect flow). Registers the AuthenticationStateProvider
// and authorization services; nothing secret is involved — this is a public client.
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("EntraExternalId", options.ProviderOptions.Authentication);

    var apiScope = builder.Configuration["EntraExternalId:ApiScope"];
    if (!string.IsNullOrWhiteSpace(apiScope))
    {
        options.ProviderOptions.DefaultAccessTokenScopes.Add(apiScope);
    }
});

// Application services — call WorkoutManager.Api over HTTP (see ApiAuthorizationMessageHandler above)
builder.Services.AddScoped<WorkoutManager.Web.Services.IWorkoutPlanService, WorkoutManager.Web.Services.WorkoutPlanService>();
builder.Services.AddScoped<WorkoutManager.Web.Services.ISessionService, WorkoutManager.Web.Services.SessionService>();
builder.Services.AddScoped<WorkoutManager.Web.Services.IExerciseService, WorkoutManager.Web.Services.ExerciseService>();
builder.Services.AddScoped<WorkoutManager.Web.Services.IMuscleGroupService, WorkoutManager.Web.Services.MuscleGroupService>();
builder.Services.AddScoped<IWorkoutSessionStateService, WorkoutSessionStateService>();

// MudBlazor
builder.Services.AddMudServices();

await builder.Build().RunAsync();
