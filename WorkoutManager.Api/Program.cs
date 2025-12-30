using FluentValidation;
using Supabase;
using WorkoutManager.Api.Services;
using WorkoutManager.BusinessLogic.Services.Implementations;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WorkoutManager.Api.HostedServices;
using WorkoutManager.Api.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Configuration.AddUserSecrets<Program>();

// Configure Supabase
var supabaseSettings = new SupabaseSettings();
builder.Configuration.GetSection(SupabaseSettings.SectionName).Bind(supabaseSettings);

builder.Services.AddScoped(_ =>
{
    var options = new SupabaseOptions
    {
        AutoConnectRealtime = false
    };
    return new Client(supabaseSettings.Url, supabaseSettings.Secret, options);
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

var jwtBearerSettings = new JwtBearerSettings();
builder.Configuration.GetSection(JwtBearerSettings.SectionName).Bind(jwtBearerSettings);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = supabaseSettings.Url;
        options.Audience = jwtBearerSettings.Audience;
        options.RequireHttpsMetadata = jwtBearerSettings.RequireHttpsMetadata;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = jwtBearerSettings.ValidateIssuer,
            ValidateAudience = jwtBearerSettings.ValidateAudience,
            ValidateLifetime = jwtBearerSettings.ValidateLifetime,
            ValidateIssuerSigningKey = jwtBearerSettings.ValidateIssuerSigningKey,
            ValidIssuer = jwtBearerSettings.ValidIssuer,
            ValidAudience = jwtBearerSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtBearerSettings.IssuerSigningKey))
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
