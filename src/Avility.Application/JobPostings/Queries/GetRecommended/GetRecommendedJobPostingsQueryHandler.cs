using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.Common.Models;
using Avility.Application.JobPostings.Dtos;
using Avility.Domain.Entities;
using Avility.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.JobPostings.Queries.GetRecommended;

/// <summary>
/// Deterministic, explainable ranking - not a black-box/AI recommender.
/// Published postings are ordered by how many of the JobSeeker's own
/// disclosed DisabilityCategories a posting's SupportedDisabilityCategories
/// overlaps with, most-overlap first; postings with zero overlap still
/// appear (just last, newest-first among themselves) - this re-ranks
/// everything Published rather than filtering it down, so a JobSeeker who
/// discloses nothing still sees every posting, just in normal order.
/// </summary>
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

        // Same reasoning as the disabilityCategory search filter: this
        // property is a converted collection column EF can't translate
        // into SQL, so the overlap/ordering happens in memory over the
        // already-Published set.
        var ranked = published
            .OrderByDescending(p => OverlapCount(p, jobSeeker))
            .ThenByDescending(p => p.PublishedAt)
            .ToList();

        var pagedItems = ranked
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedResult<JobPostingDto>(
            pagedItems.Select(p => p.ToDto()).ToList(), request.PageNumber, request.PageSize, ranked.Count);
    }

    private static int OverlapCount(JobPosting posting, JobSeeker jobSeeker) =>
        posting.SupportedDisabilityCategories.Intersect(jobSeeker.DisabilityCategories).Count();
}