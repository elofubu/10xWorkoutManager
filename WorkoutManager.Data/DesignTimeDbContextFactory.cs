using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WorkoutManager.Data;

/// <summary>
/// Used only by the dotnet ef tooling, so migrations can be generated without a live database
/// or the Api host. Override the connection with the ConnectionStrings__WorkoutManagerDb env var.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<WorkoutManagerDbContext>
{
    public WorkoutManagerDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__WorkoutManagerDb")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=WorkoutManagerDesignTime;Trusted_Connection=True;";

        var options = new DbContextOptionsBuilder<WorkoutManagerDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new WorkoutManagerDbContext(options);
    }
}
