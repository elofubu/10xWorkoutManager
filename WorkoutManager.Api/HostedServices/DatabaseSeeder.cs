namespace WorkoutManager.Api.HostedServices;

using Microsoft.EntityFrameworkCore;
using WorkoutManager.Api.Data;
using WorkoutManager.Data;
using WorkoutManager.Data.Models;

public class DatabaseSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(IServiceProvider serviceProvider, ILogger<DatabaseSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Database seeder is starting.");

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WorkoutManagerDbContext>();

        try
        {
            // Applies pending EF Core migrations, creating the schema on a fresh database.
            await db.Database.MigrateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Fail fast: running against a missing/outdated schema only produces confusing errors later.
            _logger.LogCritical(ex, "An error occurred while applying database migrations.");
            throw;
        }

        await SeedMuscleGroupsAsync(db, cancellationToken);
        await SeedExercisesAsync(db, cancellationToken);

        _logger.LogInformation("Database seeder has finished.");
    }

    private async Task SeedMuscleGroupsAsync(WorkoutManagerDbContext db, CancellationToken cancellationToken)
    {
        try
        {
            if (!await db.MuscleGroups.AnyAsync(cancellationToken))
            {
                _logger.LogInformation("Seeding muscle groups...");
                var muscleGroupsToSeed = SeedData.MuscleGroups.ToList();
                db.MuscleGroups.AddRange(muscleGroupsToSeed);
                await db.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("{Count} muscle groups seeded.", muscleGroupsToSeed.Count);
            }
            else
            {
                _logger.LogInformation("Muscle groups already seeded.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding muscle groups.");
        }
    }

    private async Task SeedExercisesAsync(WorkoutManagerDbContext db, CancellationToken cancellationToken)
    {
        try
        {
            if (!await db.Exercises.AnyAsync(e => e.UserId == null, cancellationToken))
            {
                _logger.LogInformation("Seeding predefined exercises...");
                var muscleGroupMap = await db.MuscleGroups
                    .ToDictionaryAsync(mg => mg.Name, mg => mg.Id, cancellationToken);

                var exercisesToSeed = SeedData.Exercises
                                              .Select(e => new Exercise
                                              {
                                                  Name = e.Name,
                                                  MuscleGroupId = muscleGroupMap[e.MuscleGroupName]
                                              })
                                              .ToList();

                db.Exercises.AddRange(exercisesToSeed);
                await db.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("{Count} predefined exercises seeded.", exercisesToSeed.Count);
            }
            else
            {
                _logger.LogInformation("Predefined exercises already seeded.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding predefined exercises.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
