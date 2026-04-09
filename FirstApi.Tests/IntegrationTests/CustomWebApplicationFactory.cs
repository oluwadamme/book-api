using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FirstApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace FirstApi.Tests.IntegrationTests;
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            // Provide test configuration values needed by the app
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ThisIsASecretKeyForTestingPurposes123!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpirationInMinutes"] = "60",
                ["Jwt:RefreshTokenExpirationInDays"] = "30",
                ["EmailVerification:ExpirationInMinutes"] = "30",
                // Override the connection string to prevent Npgsql from being used
                ["ConnectionStrings:DefaultConnection"] = ""
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove ALL services that reference EF Core providers to avoid conflicts
            var efServiceTypes = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<FirstApiContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(FirstApiContext) ||
                    d.ServiceType.FullName!.StartsWith("Microsoft.EntityFrameworkCore"))
                .ToList();
            foreach (var descriptor in efServiceTypes)
                services.Remove(descriptor);

            // Add an in-memory database for testing (unique name per instance)
            var dbName = $"TestDb_{Guid.NewGuid()}";
            services.AddDbContext<FirstApiContext>(options =>
                options.UseInMemoryDatabase(dbName));
        });
    }
}