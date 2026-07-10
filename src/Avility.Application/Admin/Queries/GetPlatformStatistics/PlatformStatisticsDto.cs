namespace Avility.Application.Admin.Queries.GetPlatformStatistics;

public sealed record PlatformStatisticsDto(
    int TotalJobSeekers,
    int TotalCompanies,
    int VerifiedCompanies,
    int PendingVerificationCompanies,
    int RejectedCompanies,
    int TotalJobPostings,
    int PublishedJobPostings,
    int DraftJobPostings,
    int ClosedJobPostings,
    int TotalJobApplications,
    IReadOnlyDictionary<string, int> ApplicationsByStatus,
    int ActiveUsers,
    int DeactivatedUsers);