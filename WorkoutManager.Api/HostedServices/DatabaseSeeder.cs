namespace WorkoutManager.Api.HostedServices;

using System.Linq;
using WorkoutManager.Api.Data;
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
        var supabase = scope.ServiceProvider.GetRequiredService<Supabase.Client>();
        await SeedDataAsync(supabase, cancellationToken);


        _logger.LogInformation("Database seeder has finished.");
    }

    private async Task SeedDataAsync(Supabase.Client supabase, CancellationToken cancellationToken)
    {
        await SeedMuscleGroupsAsync(supabase, cancellationToken);
        await SeedExercisesAsync(supabase, cancellationToken);
    }

    private async Task SeedMuscleGroupsAsync(Supabase.Client supabase, CancellationToken cancellationToken)
    {
        try
        {
            var muscleGroupsCountResponse = await supabase.From<MuscleGroup>().Count(Supabase.Postgrest.Constants.CountType.Exact, cancellationToken);
            if (muscleGroupsCountResponse == 0)
            {
                _logger.LogInformation("Seeding muscle groups...");
                var muscleGroupsToSeed = SeedData.MuscleGroups.ToList();
                await supabase.From<MuscleGroup>().Insert(muscleGroupsToSeed, cancellationToken: cancellationToken);
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

    private async Task SeedExercisesAsync(Supabase.Client supabase, CancellationToken cancellationToken)
    {
        try
        {
            var exercisesCountResponse = await supabase.From<Exercise>().Where(e => e.UserId == null).Count(Supabase.Postgrest.Constants.CountType.Exact, cancellationToken);
            if (exercisesCountResponse == 0)
            {
                _logger.LogInformation("Seeding predefined exercises...");
                var muscleGroups = (await supabase.From<MuscleGroup>().Get(cancellationToken)).Models;
                var muscleGroupMap = muscleGroups.ToDictionary(mg => mg.Name, mg => mg.Id);

                var exercisesToSeed = SeedData.Exercises
                                              .Select(e => new Exercise
                                              {
                                                  Name = e.Name,
                                                  MuscleGroupId = muscleGroupMap[e.MuscleGroupName]
                                              })
                                              .ToList();

                await supabase.From<Exercise>().Insert(exercisesToSeed, cancellationToken: cancellationToken);
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
