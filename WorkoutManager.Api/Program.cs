using FluentValidation;
using Supabase;
using WorkoutManager.Api.Services;
using WorkoutManager.BusinessLogic.Services.Implementations;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Api.HostedServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Configure Supabase
builder.Services.AddScoped(_ => 
{
    var supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL");
    var supabaseKey = Environment.GetEnvironmentVariable("SUPABASE_KEY");

    var options = new SupabaseOptions
    {
        AutoConnectRealtime = false
    };
    return new Client(supabaseUrl, supabaseKey, options);
});

// Register Business Logic Services
builder.Services.AddScoped<IMuscleGroupService, MuscleGroupService>();
builder.Services.AddScoped<IMuscleGroupRepository, MuscleGroupRepository>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<IExerciseRepository, ExerciseRepository>();
builder.Services.AddScoped<IWorkoutPlanService, WorkoutPlanService>();
builder.Services.AddScoped<IWorkoutPlanRepository, WorkoutPlanRepository>();
builder.Services.AddScoped<IPlanExerciseService, PlanExerciseService>();
builder.Services.AddScoped<IPlanExerciseRepository, PlanExerciseRepository>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<ISessionExerciseService, SessionExerciseService>();
builder.Services.AddScoped<ISessionExerciseRepository, SessionExerciseRepository>();

// Register API Services
builder.Services.AddScoped<IUserContextService, UserContextService>();

// Register FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<WorkoutManager.BusinessLogic.Validators.CreateExerciseCommandValidator>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Supabase:Url"];
        options.Audience = "authenticated";
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = false,
            ValidIssuer = "http://127.0.0.1:54321/auth/v1",
            //ValidIssuer = builder.Configuration["Supabase:Url"],
            ValidAudience = "authenticated",
            //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Supabase:JwtSecret"]))
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("super-secret-jwt-token-with-at-least-32-characters-long"))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHostedService<DatabaseSeeder>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseCors();
app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
