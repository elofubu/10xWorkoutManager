using BlazorBootstrap;
using FluentValidation;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WorkoutManager.Business.Interfaces;
using WorkoutManager.Business.Services;
using WorkoutManager.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.HostEnvironment.Environment}.json", optional: true, reloadOnChange: true);

var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:Key"];

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped(provider => new Supabase.Client(supabaseUrl!, supabaseKey, new Supabase.SupabaseOptions { AutoRefreshToken = true, AutoConnectRealtime = true }));

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<WorkoutManager.Business.Mappers.AutoMapperProfile>());
builder.Services.AddValidatorsFromAssembly(typeof(WorkoutManager.Business.Validators.CreateExerciseDtoValidator).Assembly);

builder.Services.AddScoped<IWorkoutPlanService, WorkoutPlanService>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<ITrainingDayService, TrainingDayService>();

builder.Services.AddBlazorBootstrap();

await builder.Build().RunAsync();
