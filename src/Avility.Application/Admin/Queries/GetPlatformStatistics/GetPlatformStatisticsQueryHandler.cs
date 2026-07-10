using Avility.Application.Common.Interfaces;
using Avility.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Admin.Queries.GetPlatformStatistics;

public sealed class GetPlatformStatisticsQueryHandler : IRequestHandler<GetPlatformStatisticsQuery, PlatformStatisticsDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IIdentityService _identityService;

    public GetPlatformStatisticsQueryHandler(IApplicationDbContext dbContext, IIdentityService identityService)
    {
        _dbContext = dbContext;
        _identityService = identityService;
    }

    public async Task<PlatformStatisticsDto> Handle(GetPlatformStatisticsQuery request, CancellationToken cancellationToken)
    {
        var totalJobSeekers = await _dbContext.JobSeekers.CountAsync(cancellationToken);

        var companyCounts = await _dbContext.Companies
            .GroupBy(c => c.VerificationStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        var verified = companyCounts.FirstOrDefault(x => x.Status == CompanyVerificationStatus.Verified)?.Count ?? 0;
        var pending = companyCounts.FirstOrDefault(x => x.Status == CompanyVerificationStatus.Pending)?.Count ?? 0;
        var rejected = companyCounts.FirstOrDefault(x => x.Status == CompanyVerificationStatus.Rejected)?.Count ?? 0;

        var postingCounts = await _dbContext.JobPostings
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        var published = postingCounts.FirstOrDefault(x => x.Status == JobPostingStatus.Published)?.Count ?? 0;
        var draft = postingCounts.FirstOrDefault(x => x.Status == JobPostingStatus.Draft)?.Count ?? 0;
        var closed = postingCounts.FirstOrDefault(x => x.Status == JobPostingStatus.Closed)?.Count ?? 0;

        var applicationCounts = await _dbContext.JobApplications
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        var applicationsByStatus = applicationCounts.ToDictionary(x => x.Status.ToString(), x => x.Count);

        var (activeUsers, deactivatedUsers) = await _identityService.GetUserCountsAsync();

        return new PlatformStatisticsDto(
            totalJobSeekers,
            companyCounts.Sum(x => x.Count),
            verified,
            pending,
            rejected,
            postingCounts.Sum(x => x.Count),
            published,
            draft,
            closed,
            applicationCounts.Sum(x => x.Count),
            applicationsByStatus,
            activeUsers,
            deactivatedUsers);
    }
}