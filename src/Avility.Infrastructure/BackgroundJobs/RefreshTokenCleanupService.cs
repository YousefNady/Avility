using Avility.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Avility.Infrastructure.BackgroundJobs;

/// <summary>
/// Periodically hard-deletes expired refresh tokens. RefreshToken has no
/// soft-delete (it inherits BaseEntity, not AuditableEntity - see its own
/// doc comment), so a hard delete here loses no audit trail beyond what
/// the token already stops being useful for. Runs once at startup, then
/// on a fixed interval via PeriodicTimer - no external scheduler package.
/// </summary>
public sealed class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RefreshTokenCleanupService> _logger;
    private readonly TimeSpan _interval;

    public RefreshTokenCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<RefreshTokenCleanupService> logger,
        IOptions<BackgroundJobsSettings> settings)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _interval = TimeSpan.FromHours(settings.Value.RefreshTokenCleanupIntervalHours);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval);
        do
        {
            await CleanupExpiredTokensAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    /// <summary>
    /// Public and directly callable (not just via the timer loop) so it
    /// can be exercised deterministically in tests without waiting on a
    /// real interval.
    /// </summary>
    public async Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var dateTime = scope.ServiceProvider.GetRequiredService<IDateTime>();

        var expired = await dbContext.RefreshTokens
            .Where(t => t.ExpiresAt < dateTime.UtcNow)
            .ToListAsync(cancellationToken);

        if (expired.Count == 0)
        {
            return 0;
        }

        dbContext.RefreshTokens.RemoveRange(expired);
        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Removed {Count} expired refresh token(s).", expired.Count);
        return expired.Count;
    }
}