using FluentValidation;
using Microsoft.EntityFrameworkCore;
using WorkoutManager.Data;
using WorkoutManager.Api.Services;
using WorkoutManager.BusinessLogic.Services.Implementations;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WorkoutManager.Api.HostedServices;
using WorkoutManager.Api.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Configuration.AddUserSecrets<Program>();
builder.Services.AddDbContext<WorkoutManagerDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("WorkoutManagerDb"),
        sql => sql.EnableRetryOnFailure()));

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

// Microsoft Entra External ID: tokens are validated against the tenant's OIDC metadata
// (signing keys are discovered from the Authority), so there is no shared secret to configure.
var entraSettings = new EntraExternalIdSettings();
builder.Configuration.GetSection(EntraExternalIdSettings.SectionName).Bind(entraSettings);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = entraSettings.Authority;
        options.Audience = entraSettings.Audience;
        // Keep claim names as issued ("oid", "email") rather than remapping to legacy URIs.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
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
