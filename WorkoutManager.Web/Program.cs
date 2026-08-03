using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using WorkoutManager.Web;
using WorkoutManager.Web.Components;
using WorkoutManager.Web.Services;
using Supabase;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Browser-native storage (replaces Blazor Server's ProtectedLocalStorage/ProtectedSessionStorage)
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredSessionStorage();

// HTTP client for WorkoutManager.Api — every request gets the Supabase access token attached
var apiUrl = builder.Configuration["ApiUrl"] ?? builder.HostEnvironment.BaseAddress;
if (!apiUrl.EndsWith('/')) apiUrl += "/"; // relative request URIs (e.g. "api/exercises") need a trailing slash on the base

builder.Services.AddTransient<AuthorizationMessageHandler>();
builder.Services.AddHttpClient("ApiHttpClient", client => client.BaseAddress = new Uri(apiUrl))
    .AddHttpMessageHandler<AuthorizationMessageHandler>();
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiHttpClient"));

// Authentication
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, SupabaseAuthenticationStateProvider>();

// Supabase client — runs client-side for Auth only. Anon key is safe to ship in WASM;
// the secret/service-role key stays server-side in WorkoutManager.Api.
builder.Services.AddScoped(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var supabaseUrl = configuration["Supabase:Url"];
    var supabaseAnonKey = configuration["Supabase:AnonKey"];
    var options = new SupabaseOptions
    {
        AutoConnectRealtime = false
    };
    return new Client(supabaseUrl, supabaseAnonKey, options);
});

// Application services — call WorkoutManager.Api over HTTP (see AuthorizationMessageHandler above)
builder.Services.AddScoped<WorkoutManager.Web.Services.IWorkoutPlanService, WorkoutManager.Web.Services.WorkoutPlanService>();
builder.Services.AddScoped<WorkoutManager.Web.Services.ISessionService, WorkoutManager.Web.Services.SessionService>();
builder.Services.AddScoped<WorkoutManager.Web.Services.IExerciseService, WorkoutManager.Web.Services.ExerciseService>();
builder.Services.AddScoped<WorkoutManager.Web.Services.IAuthService, WorkoutManager.Web.Services.AuthService>();
builder.Services.AddScoped<WorkoutManager.Web.Services.IMuscleGroupService, WorkoutManager.Web.Services.MuscleGroupService>();
builder.Services.AddScoped<IWorkoutSessionStateService, WorkoutSessionStateService>();

// MudBlazor
builder.Services.AddMudServices();

await builder.Build().RunAsync();
