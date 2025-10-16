using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Respawn;
using Respawn.Graph;
using WorkoutManager.Api.Services;
using WorkoutManager.Api.Tests.Settings;
using Microsoft.AspNetCore.Http;

namespace WorkoutManager.Api.Tests;

public class TestUserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Guid? _overrideUserId;
    private string? _overrideUserEmail;

    public TestUserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void SetUserId(Guid userId)
    {
        _overrideUserId = userId;
    }

    public void SetUserEmail(string email)
    {
        _overrideUserEmail = email;
    }

    public Guid GetCurrentUserId()
    {
        // Use override if set (for backwards compatibility)
        if (_overrideUserId.HasValue)
        {
            return _overrideUserId.Value;
        }

        // Read from JWT claims in HttpContext
        var userId = _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidOperationException("User ID not found in token claims or test override.");
        }

        return Guid.Parse(userId);
    }

    public string? GetCurrentUserEmail()
    {
        // Use override if set
        if (_overrideUserEmail != null)
        {
            return _overrideUserEmail;
        }

        // Read from JWT claims in HttpContext
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
    }
}

public abstract class BaseIntegrationTest : IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly IConfiguration _configuration;
    private readonly IntegrationTestDatabaseFixture _databaseFixture;
    private readonly SupabaseSettings _supabaseSettings;

    protected HttpClient HttpClient { get; }

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory, IntegrationTestDatabaseFixture databaseFixture)
    {
        _factory = factory;
        _databaseFixture = databaseFixture;
        _configuration = factory.Services.GetRequiredService<IConfiguration>();
        _supabaseSettings = _configuration.GetSection(SupabaseSettings.SectionName).Get<SupabaseSettings>()
            ?? throw new InvalidOperationException("Failed to load Supabase settings from configuration.");

        HttpClient = _factory.CreateClient();
    }

    protected Guid UserId => _supabaseSettings.TestUserId;

    protected void Authenticate()
    {
        var token = GenerateJwtToken();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private string GenerateJwtToken()
    {
        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new InvalidOperationException("JWT Key not found in configuration.");
        }
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _supabaseSettings.TestUserId.ToString()),
            new Claim("user_id", _supabaseSettings.TestUserId.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task InitializeAsync()
    {
        // Initialize the database fixture with connection string on first run
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            _databaseFixture.Initialize(connectionString);
            // Ensure the connection is created if this is the first test
            await _databaseFixture.EnsureInitializedAsync();
        }

        // Reset database before each test to ensure clean state
        try
        {
            if (_databaseFixture.Connection != null && _databaseFixture.Connection.State == System.Data.ConnectionState.Open)
            {
                await _databaseFixture.Respawner.ResetAsync(_databaseFixture.Connection);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error resetting database before test: {ex.Message}");
            throw;
        }
    }

    public async Task DisposeAsync()
    {
        // Database cleanup is handled by IntegrationTestDatabaseFixture after all tests complete
        // This method is kept for IAsyncLifetime contract compliance
        await Task.CompletedTask;
    }
}

