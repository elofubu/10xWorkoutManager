using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using MudBlazor.Services;
using WorkoutManager.Web;
using WorkoutManager.Web.Components;
using WorkoutManager.Web.Services;
using Supabase;

var builder = WebApplication.CreateBuilder(args);

// Configure Data Protection with persistent keys
var keysDirectory = new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys"));
if (!keysDirectory.Exists)
{
    keysDirectory.Create();
}

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(keysDirectory)
    .SetApplicationName("WorkoutManager");

// Add Blazor Server services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(180);
        options.KeepAliveInterval = TimeSpan.FromSeconds(90);
    });

// Authentication
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, SupabaseAuthenticationStateProvider>();

// Supabase client for data access
builder.Services.AddScoped(_ =>
{
    var supabaseUrl = builder.Configuration["Supabase:Url"];
    var supabaseSecret = builder.Configuration["Supabase:Secret"];
    var options = new SupabaseOptions
    {
        AutoConnectRealtime = false
    };
    return new Client(supabaseUrl, supabaseSecret, options);
});

// Register Business Logic Services (repositories and core services)
builder.Services.AddScoped<WorkoutManager.BusinessLogic.Services.Interfaces.IMuscleGroupService, WorkoutManager.BusinessLogic.Services.Implementations.MuscleGroupService>();
builder.Services.AddScoped<WorkoutManager.BusinessLogic.Services.Interfaces.IMuscleGroupRepository, WorkoutManager.BusinessLogic.Services.Implementations.MuscleGroupRepository>();
builder.Services.AddScoped<WorkoutManager.BusinessLogic.Services.Interfaces.IExerciseService, WorkoutManager.BusinessLogic.Services.Implementations.ExerciseService>();
builder.Services.AddScoped<WorkoutManager.BusinessLogic.Services.Interfaces.IExerciseRepository, WorkoutManager.BusinessLogic.Services.Implementations.ExerciseRepository>();
builder.Services.AddScoped<WorkoutManager.BusinessLogic.Services.Interfaces.IWorkoutPlanService, WorkoutManager.BusinessLogic.Services.Implementations.WorkoutPlanService>();
builder.Services.AddScoped<WorkoutManager.BusinessLogic.Services.Interfaces.IWorkoutPlanRepository, WorkoutManager.BusinessLogic.Services.Implementations.WorkoutPlanRepository>();
builder.Services.AddScoped<WorkoutManager.BusinessLogic.Services.Interfaces.IPlanExerciseService, WorkoutManager.BusinessLogic.Services.Implementations.PlanExerciseService>();
builder.Services.AddScoped<WorkoutManager.BusinessLogic.Services.Interfaces.IPlanExerciseRepository, WorkoutManager.BusinessLogic.Services.Implementations.PlanExerciseRepository>();
builder.Services.AddScoped<WorkoutManager.BusinessLogic.Services.Interfaces.ISessionService, WorkoutManager.BusinessLogic.Services.Implementations.SessionService>();
builder.Services.AddScoped<WorkoutManager.BusinessLogic.Services.Interfaces.ISessionRepository, WorkoutManager.BusinessLogic.Services.Implementations.SessionRepository>();
builder.Services.AddScoped<WorkoutManager.BusinessLogic.Services.Interfaces.ISessionExerciseService, WorkoutManager.BusinessLogic.Services.Implementations.SessionExerciseService>();
builder.Services.AddScoped<WorkoutManager.BusinessLogic.Services.Interfaces.ISessionExerciseRepository, WorkoutManager.BusinessLogic.Services.Implementations.SessionExerciseRepository>();

// Application services (wrappers that extract userId and call business logic)
builder.Services.AddScoped<WorkoutManager.Web.Services.IWorkoutPlanService, WorkoutManager.Web.Services.WorkoutPlanService>();
builder.Services.AddScoped<WorkoutManager.Web.Services.ISessionService, WorkoutManager.Web.Services.SessionService>();
builder.Services.AddScoped<WorkoutManager.Web.Services.IExerciseService, WorkoutManager.Web.Services.ExerciseService>();
builder.Services.AddScoped<WorkoutManager.Web.Services.IAuthService, WorkoutManager.Web.Services.AuthService>();
builder.Services.AddScoped<WorkoutManager.Web.Services.IMuscleGroupService, WorkoutManager.Web.Services.MuscleGroupService>();
builder.Services.AddScoped<IWorkoutSessionStateService, WorkoutSessionStateService>();

// MudBlazor
builder.Services.AddMudServices();

var app = builder.Build();

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
