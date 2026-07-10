using Avility.Application.Common.Extensions;
using Avility.Application.Common.Interfaces;
using Avility.Application.Common.Models;
using Avility.Application.JobPostings.Dtos;
using Avility.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.JobPostings.Queries.Search;

public sealed class SearchJobPostingsQueryHandler : IRequestHandler<SearchJobPostingsQuery, PagedResult<JobPostingDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public SearchJobPostingsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<JobPostingDto>> Handle(SearchJobPostingsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.JobPostings.AsNoTracking().Where(p => p.Status == JobPostingStatus.Published);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(p => p.Title.Contains(request.Search));
        }

        if (!string.IsNullOrWhiteSpace(request.EmploymentType) && Enum.TryParse<EmploymentType>(request.EmploymentType, out var employmentType))
        {
            query = query.Where(p => p.EmploymentType == employmentType);
        }

        if (!string.IsNullOrWhiteSpace(request.ExperienceLevel) && Enum.TryParse<ExperienceLevel>(request.ExperienceLevel, out var experienceLevel))
        {
            query = query.Where(p => p.ExperienceLevel == experienceLevel);
        }

        if (request.IsRemote.HasValue)
        {
            query = query.Where(p => p.IsRemote == request.IsRemote.Value);
        }

        query = query.OrderByDescending(p => p.PublishedAt);

        if (!string.IsNullOrWhiteSpace(request.DisabilityCategory) &&
            Enum.TryParse<DisabilityCategory>(request.DisabilityCategory, out var category))
        {
            // SupportedDisabilityCategories is stored as a converted
            // collection (comma-separated string column) - EF Core can't
            // translate a LINQ filter against a converted collection
            // property into SQL. The other filters above have already
            // narrowed the result set at the DB level, so this one
            // optional filter is applied in memory against what's left.
            // Reasonable at this project's scale; a high-volume version
            // would normalize this into its own join table instead.
            var candidates = await query.ToListAsync(cancellationToken);
            var filtered = candidates.Where(p => p.SupportedDisabilityCategories.Contains(category)).ToList();

            var pagedItems = filtered
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PagedResult<JobPostingDto>(
                pagedItems.Select(p => p.ToDto()).ToList(), request.PageNumber, request.PageSize, filtered.Count);
        }

        var page = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResult<JobPostingDto>(
            page.Items.Select(p => p.ToDto()).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }
}
