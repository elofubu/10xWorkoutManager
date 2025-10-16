using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Supabase;
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
            var supabaseSettings = new SupabaseSettings();
            builderContext.Configuration.GetSection(SupabaseSettings.SectionName).Bind(supabaseSettings);
            services.Configure<SupabaseSettings>((configure) =>
            {
                configure = supabaseSettings;
            });

            services.Remove(services.SingleOrDefault(s => s.ServiceType == typeof(Client)));

            services.AddSingleton(provider =>
            {
                var options = new Supabase.SupabaseOptions
                {
                    AutoRefreshToken = false,
                    AutoConnectRealtime = false,
                };

                var client = new Client(supabaseSettings.Url, supabaseSettings.AnonKey, options);
                return client;
            });

            services.AddSingleton<IUserContextService, TestUserContextService>();

            // Override JWT configuration for tests to accept test-generated tokens
            var jwtKey = builderContext.Configuration["Jwt:Key"];
            var jwtIssuer = builderContext.Configuration["Jwt:Issuer"];
            var jwtAudience = builderContext.Configuration["Jwt:Audience"];

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

