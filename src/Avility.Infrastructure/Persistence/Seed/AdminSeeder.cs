using Avility.Application.Common.Constants;
using Avility.Application.Common.Interfaces;
using Avility.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Avility.Infrastructure.Persistence.Seed;

/// <summary>
/// Bootstraps exactly one Admin account from configuration on startup, so
/// there's no need to manually promote a user via UserManager after
/// every fresh deployment/database. Skips silently (not an error) if
/// Seed:AdminEmail/Seed:AdminPassword aren't configured, or if an Admin
/// already exists - safe to leave running on every startup indefinitely,
/// and safe to leave unconfigured anywhere that doesn't need it (e.g.
/// the integration test suite, which creates its own admins per-test).
/// </summary>
public static class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder");

        var email = configuration["Seed:AdminEmail"];
        var password = configuration["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogInformation("Admin seeding skipped - Seed:AdminEmail/Seed:AdminPassword not configured.");
            return;
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var existingAdmins = await userManager.GetUsersInRoleAsync(Roles.Admin);
        if (existingAdmins.Count > 0)
        {
            logger.LogInformation("Admin seeding skipped - an Admin account already exists.");
            return;
        }

        var identityService = services.GetRequiredService<IIdentityService>();
        var (succeeded, _, errors) = await identityService.CreateUserAsync(email, password, Roles.Admin);

        if (!succeeded)
        {
            logger.LogError("Admin seeding failed: {Errors}", string.Join(" ", errors));
            return;
        }

        logger.LogInformation("Seeded initial Admin account for {Email}.", email);
    }
}