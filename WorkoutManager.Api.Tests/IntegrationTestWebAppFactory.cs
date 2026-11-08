using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WorkoutManager.Api.Services;
using WorkoutManager.Api.Tests.Settings;

namespace WorkoutManager.Api.Tests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var projectDir = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(projectDir, "appsettings.Testing.json");

            config.AddJsonFile(configPath);
            config.AddUserSecrets<IntegrationTestWebAppFactory>();


        });

        builder.ConfigureServices((builderContext, services) =>
        {
            services.AddSingleton<IUserContextService, TestUserContextService>();

            // Override JWT configuration for tests to accept test-generated tokens
            var jwtKey = builderContext.Configuration["JwtBearerSettings:Key"];
            var jwtIssuer = builderContext.Configuration["JwtBearerSettings:Issuer"];
            var jwtAudience = builderContext.Configuration["JwtBearerSettings:Audience"];

            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = null; // Don't use authority for tests
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? ""))
                };
            });
        });
    }
}

