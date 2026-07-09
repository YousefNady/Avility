namespace Avility.Infrastructure.BackgroundJobs;

public sealed class BackgroundJobsSettings
{
    public const string SectionName = "BackgroundJobs";

    public int RefreshTokenCleanupIntervalHours { get; init; } = 24;
}