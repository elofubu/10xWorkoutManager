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

// HTTP client for WorkoutManager.Api
// TODO (Phase 4): wrap with an AuthorizationMessageHandler that attaches the Supabase JWT
builder.Services.AddHttpClient("ApiHttpClient", client =>
    client.BaseAddress = new Uri(builder.Configuration["ApiUrl"] ?? builder.HostEnvironment.BaseAddress));
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

// NOTE: BusinessLogic service/repository registrations removed — that project talks to
// Supabase with the secret key and cannot be referenced from WASM. The application services
// below currently still call into BusinessLogic types and will not compile until Phase 4
// rewrites them to call WorkoutManager.Api over HTTP instead.

// Application services (Phase 4 will rewrite these to call WorkoutManager.Api over HTTP)
builder.Services.AddScoped<WorkoutManager.Web.Services.IWorkoutPlanService, WorkoutManager.Web.Services.WorkoutPlanService>();
builder.Services.AddScoped<WorkoutManager.Web.Services.ISessionService, WorkoutManager.Web.Services.SessionService>();
builder.Services.AddScoped<WorkoutManager.Web.Services.IExerciseService, WorkoutManager.Web.Services.ExerciseService>();
builder.Services.AddScoped<WorkoutManager.Web.Services.IAuthService, WorkoutManager.Web.Services.AuthService>();
builder.Services.AddScoped<WorkoutManager.Web.Services.IMuscleGroupService, WorkoutManager.Web.Services.MuscleGroupService>();
builder.Services.AddScoped<IWorkoutSessionStateService, WorkoutSessionStateService>();

// MudBlazor
builder.Services.AddMudServices();

await builder.Build().RunAsync();
