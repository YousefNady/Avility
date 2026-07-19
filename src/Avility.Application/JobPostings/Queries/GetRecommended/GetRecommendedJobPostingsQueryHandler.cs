using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.Common.Models;
using Avility.Application.JobPostings.Dtos;
using Avility.Domain.Entities;
using Avility.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.JobPostings.Queries.GetRecommended;

public sealed class GetRecommendedJobPostingsQueryHandler : IRequestHandler<GetRecommendedJobPostingsQuery, PagedResult<JobPostingDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public GetRecommendedJobPostingsQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<JobPostingDto>> Handle(GetRecommendedJobPostingsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        var jobSeeker = await _dbContext.JobSeekers
            .AsNoTracking()
            .FirstOrDefaultAsync(js => js.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("JobSeeker profile", userId);

        var published = await _dbContext.JobPostings
            .AsNoTracking()
            .Where(p => p.Status == JobPostingStatus.Published)
            .ToListAsync(cancellationToken);

        var ranked = published
            .OrderByDescending(p => OverlapCount(p, jobSeeker))
            .ThenByDescending(p => p.PublishedAt)
            .ToList();

        var pagedItems = ranked
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var companyIds = pagedItems.Select(p => p.CompanyId).Distinct().ToList();
        var companies = await _dbContext.Companies.AsNoTracking()
            .Where(c => companyIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        return new PagedResult<JobPostingDto>(
            pagedItems.Select(p => p.ToDto(companies[p.CompanyId])).ToList(), request.PageNumber, request.PageSize, ranked.Count);
    }

    private static int OverlapCount(JobPosting posting, JobSeeker jobSeeker) =>
        posting.SupportedDisabilityCategories.Intersect(jobSeeker.DisabilityCategories).Count();
}