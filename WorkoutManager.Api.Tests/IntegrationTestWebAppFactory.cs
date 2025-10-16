using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Supabase;
using WorkoutManager.Api.Services;

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
        });

        builder.ConfigureServices((builderContext, services) =>
        {
            var supabaseUrl = builderContext.Configuration["Supabase:Url"];
            var supabaseKey = builderContext.Configuration["Supabase:AnonKey"];
            
            services.Remove(services.SingleOrDefault(s => s.ServiceType == typeof(Client)));

            services.AddSingleton(provider =>
            {
                var options = new Supabase.SupabaseOptions
                {
                    AutoRefreshToken = false,
                    AutoConnectRealtime = false,
                };
                
                var client = new Client(supabaseUrl, supabaseKey, options);
                return client;
            });

            services.AddScoped<IUserContextService, TestUserContextService>();

        });
    }
}

