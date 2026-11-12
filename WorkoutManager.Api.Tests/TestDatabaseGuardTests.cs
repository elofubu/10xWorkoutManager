using FluentAssertions;

namespace WorkoutManager.Api.Tests;

public class TestDatabaseGuardTests
{
    [Theory]
    [InlineData("Server=(localdb)\\MSSQLLocalDB;Database=WorkoutManagerTests;Trusted_Connection=True;")]
    [InlineData("Server=tcp:srv.database.windows.net,1433;Initial Catalog=workout-manager-test;User ID=u;Password=p;")]
    public void Allows_databases_with_test_in_the_name(string connectionString)
    {
        var act = () => TestDatabaseGuard.EnsureIsTestDatabase(connectionString);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("Server=tcp:srv.database.windows.net,1433;Initial Catalog=workout-manager;User ID=u;Password=p;")]
    [InlineData("Server=tcp:srv.database.windows.net,1433;User ID=u;Password=p;")]
    [InlineData("")]
    [InlineData(null)]
    public void Refuses_databases_that_are_not_clearly_test_databases(string? connectionString)
    {
        var act = () => TestDatabaseGuard.EnsureIsTestDatabase(connectionString);

        act.Should().Throw<InvalidOperationException>();
    }
}
