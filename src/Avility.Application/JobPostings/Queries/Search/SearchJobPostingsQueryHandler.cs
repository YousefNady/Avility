using Avility.Application.Common.Extensions;
using Avility.Application.Common.Interfaces;
using Avility.Application.Common.Models;
using Avility.Application.JobPostings.Dtos;
using Avility.Domain.Entities;
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
            var candidates = await query.ToListAsync(cancellationToken);
            var filtered = candidates.Where(p => p.SupportedDisabilityCategories.Contains(category)).ToList();

            var pagedItems = filtered
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var companiesForPage = await LoadCompaniesAsync(pagedItems, cancellationToken);

            return new PagedResult<JobPostingDto>(
                pagedItems.Select(p => p.ToDto(companiesForPage[p.CompanyId])).ToList(),
                request.PageNumber, request.PageSize, filtered.Count);
        }

        var page = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
        var companies = await LoadCompaniesAsync(page.Items, cancellationToken);

        return new PagedResult<JobPostingDto>(
            page.Items.Select(p => p.ToDto(companies[p.CompanyId])).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    /// <summary>
    /// Batch-loads the distinct companies behind a page of postings in
    /// one query rather than one query per row - JobPostingDto now
    /// carries lightweight company info so the frontend doesn't need a
    /// second request per job card.
    /// </summary>
    private async Task<Dictionary<Guid, Company>> LoadCompaniesAsync(IReadOnlyCollection<JobPosting> postings, CancellationToken cancellationToken)
    {
        var companyIds = postings.Select(p => p.CompanyId).Distinct().ToList();
        return await _dbContext.Companies.AsNoTracking()
            .Where(c => companyIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);
    }
}