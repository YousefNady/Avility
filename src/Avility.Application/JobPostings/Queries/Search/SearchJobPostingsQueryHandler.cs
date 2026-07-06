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

        var page = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResult<JobPostingDto>(
            page.Items.Select(p => p.ToDto()).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }
}
