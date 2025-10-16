namespace WorkoutManager.Api.Tests;

/// <summary>
/// Defines a collection fixture that runs all integration tests with a shared database connection.
/// This ensures one connection is created at the start and disposed only after all tests complete.
/// </summary>
[CollectionDefinition("Integration Tests")]
public class IntegrationTestsCollection : ICollectionFixture<IntegrationTestWebAppFactory>, ICollectionFixture<IntegrationTestDatabaseFixture>
{
    // This class has no code - it just defines the collection and its fixtures
}
