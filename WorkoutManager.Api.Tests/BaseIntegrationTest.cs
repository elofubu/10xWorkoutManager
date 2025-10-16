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

namespace WorkoutManager.Api.Tests;

public class TestUserContextService : IUserContextService
{
    private Guid? _userId;
    private string? _userEmail;

    public void SetUserId(Guid userId)
    {
        _userId = userId;
    }

    public void SetUserEmail(string email)
    {
        _userEmail = email;
    }

    public Guid GetCurrentUserId()
    {
        return _userId ?? throw new InvalidOperationException("User ID not set for test");
    }

    public string? GetCurrentUserEmail()
    {
        return _userEmail;
    }
}

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly IConfiguration _configuration;
    private Respawner _respawner = null!;

    protected HttpClient HttpClient { get; }

    public static class TestUsers
    {
        public static readonly Guid UserAId = Guid.NewGuid();
        public static readonly Guid UserBId = Guid.NewGuid();
    }
    
    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _configuration = factory.Services.GetRequiredService<IConfiguration>();
        HttpClient = _factory.CreateClient();
    }

    protected void AuthenticateAs(Guid userId)
    {
        var token = GenerateJwtToken(userId);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using (var scope = _factory.Services.CreateScope())
        {
            var userContext = scope.ServiceProvider.GetRequiredService<IUserContextService>() as TestUserContextService;
            userContext.SetUserId(userId);
        }
    }

    private string GenerateJwtToken(Guid userId)
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
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("user_id", userId.ToString()), 
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
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" },
            TablesToIgnore = new Table[] { new Table("muscle_groups") }
        });
    }

    public async Task DisposeAsync()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        await _respawner.ResetAsync(connectionString!);
    }
}

