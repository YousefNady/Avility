using Avility.Infrastructure.Persistence;
using Avility.Infrastructure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Avility.API.IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"AvilityTestDb-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();

            // Remove every EF provider registration (Sqlite)
            var descriptors = services
                .Where(d =>
                    d.ServiceType.Namespace != null &&
                    d.ServiceType.Namespace.StartsWith("Microsoft.EntityFrameworkCore"))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Register InMemory instead
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });

            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();

            var provider = scope.ServiceProvider;

            var context = provider.GetRequiredService<ApplicationDbContext>();

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            SeedRolesAsync(provider).GetAwaiter().GetResult();
        });
    }

    private static async Task SeedRolesAsync(IServiceProvider provider)
    {
        var roleManager = provider.GetRequiredService<RoleManager<ApplicationRole>>();

        string[] roles =
        {
    "JobSeeker",
    "Company",
    "Admin"
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = role,
                    NormalizedName = role.ToUpperInvariant()
                });
            }
        }
    }
}