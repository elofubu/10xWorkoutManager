using Microsoft.Data.SqlClient;

namespace WorkoutManager.Api.Tests;

/// <summary>
/// The integration tests wipe every table (Respawn) before each test. This guard makes sure they can
/// only ever run against a database that is clearly meant for tests, so pointing the test connection
/// string at a real database fails loudly instead of deleting real data.
/// </summary>
public static class TestDatabaseGuard
{
    public static void EnsureIsTestDatabase(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "No test database connection string configured (ConnectionStrings:WorkoutManagerDb).");
        }

        var database = new SqlConnectionStringBuilder(connectionString).InitialCatalog;

        if (string.IsNullOrWhiteSpace(database) || !database.Contains("test", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Refusing to run the integration tests against database '{database}': the tests delete all data, " +
                "so the database name must contain \"test\" (for example WorkoutManagerTests). " +
                "Point ConnectionStrings:WorkoutManagerDb at a dedicated test database.");
        }
    }
}
