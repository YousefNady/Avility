using Avility.Application.Common.Interfaces;
using Avility.Domain.Entities;
using Avility.Infrastructure.BackgroundJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Avility.API.IntegrationTests;

public class RefreshTokenCleanupTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RefreshTokenCleanupTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CleanupExpiredTokensAsync_RemovesOnlyExpiredTokens()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var dateTime = scope.ServiceProvider.GetRequiredService<IDateTime>();

        // InMemory provider doesn't enforce FK constraints, so an
        // unrelated Guid is fine - this test targets the cleanup
        // mechanism itself, not user/token association.
        var expiredToken = RefreshToken.Create(
            Guid.NewGuid(), "expired-hash", dateTime.UtcNow.AddDays(-1), dateTime.UtcNow.AddDays(-8));
        var activeToken = RefreshToken.Create(
            Guid.NewGuid(), "active-hash", dateTime.UtcNow.AddDays(7), dateTime.UtcNow);

        dbContext.RefreshTokens.AddRange(expiredToken, activeToken);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var cleanupService = scope.ServiceProvider.GetServices<IHostedService>()
            .OfType<RefreshTokenCleanupService>()
            .Single();

        var removedCount = await cleanupService.CleanupExpiredTokensAsync(CancellationToken.None);

        Assert.True(removedCount >= 1);
        Assert.False(await dbContext.RefreshTokens.AnyAsync(t => t.Id == expiredToken.Id));
        Assert.True(await dbContext.RefreshTokens.AnyAsync(t => t.Id == activeToken.Id));
    }
}