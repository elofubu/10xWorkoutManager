using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Respawn.Graph;

namespace WorkoutManager.Api.Tests;

/// <summary>
/// Shared database fixture for all integration tests in the collection.
/// Manages a single connection and respawner for the entire test run.
/// </summary>
public class IntegrationTestDatabaseFixture : IAsyncLifetime
{
    private NpgsqlConnection _connection = null!;
    private Respawner _respawner = null!;
    private string _connectionString = null!;
    private readonly object _lock = new object();
    private bool _isInitialized = false;

    public NpgsqlConnection Connection => _connection;
    public Respawner Respawner => _respawner;

    /// <summary>
    /// Ensures the database fixture is initialized.
    /// Call this after Initialize(connectionString) to create the actual connection.
    /// </summary>
    public async Task EnsureInitializedAsync()
    {
        await InitializeAsync();
    }

    /// <summary>
    /// Initialize the database fixture with a connection string.
    /// Called by the first test class that uses this fixture.
    /// This is idempotent - multiple calls with the same string are safe.
    /// </summary>
    public void Initialize(string connectionString)
    {
        lock (_lock)
        {
            if (_isInitialized && _connectionString == connectionString)
            {
                // Already initialized with the same connection string
                return;
            }

            if (_isInitialized && _connectionString != connectionString)
            {
                throw new InvalidOperationException(
                    $"Database fixture was already initialized with a different connection string. " +
                    $"Expected: {_connectionString}, Got: {connectionString}");
            }

            _connectionString = connectionString;
            _isInitialized = true;
        }
    }

    public async Task InitializeAsync()
    {
        // Lazy initialization: if connection string hasn't been set yet, skip
        // This will be called by xUnit before test classes run, but the connection string
        // will be set when the first test class initializes
        if (string.IsNullOrEmpty(_connectionString))
        {
            return;
        }

        // Only initialize once
        if (_connection != null)
        {
            return;
        }

        _connection = new NpgsqlConnection(_connectionString);
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" },
            TablesToIgnore = new Table[] { new Table("muscle_groups") }
        });
    }

    public async Task DisposeAsync()
    {
        try
        {
            // Final cleanup
            if (_connection?.State == System.Data.ConnectionState.Open)
            {
                await _respawner.ResetAsync(_connection);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during final database reset: {ex.Message}");
        }
        finally
        {
            if (_connection != null)
            {
                await _connection.CloseAsync();
                _connection.Dispose();
            }
        }
    }
}
