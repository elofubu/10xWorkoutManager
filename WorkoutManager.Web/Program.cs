using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using WorkoutManager.Web;
using WorkoutManager.Web.Services;
using Supabase;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<AuthorizationMessageHandler>();

builder.Services.AddHttpClient("ApiHttpClient", client => client.BaseAddress = new Uri(builder.Configuration["ApiUrl"]))
    .AddHttpMessageHandler<AuthorizationMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiHttpClient"));


builder.Services.AddScoped<IWorkoutPlanService, WorkoutPlanService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMuscleGroupService, MuscleGroupService>();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, SupabaseAuthenticationStateProvider>();


builder.Services.AddBlazoredLocalStorage();

builder.Services.AddSingleton(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var supabaseUrl = configuration["Supabase:Url"];
    var supabaseKey = configuration["Supabase:AnonKey"];
    return new Client(supabaseUrl, supabaseKey);
});

builder.Services.AddMudServices();

await builder.Build().RunAsync();
