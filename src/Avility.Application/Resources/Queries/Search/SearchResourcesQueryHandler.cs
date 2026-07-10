using Avility.Application.Common.Extensions;
using Avility.Application.Common.Interfaces;
using Avility.Application.Common.Models;
using Avility.Application.Resources.Dtos;
using Avility.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Resources.Queries.Search;

public sealed class SearchResourcesQueryHandler : IRequestHandler<SearchResourcesQuery, PagedResult<ResourceDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public SearchResourcesQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ResourceDto>> Handle(SearchResourcesQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Resources.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Category) && Enum.TryParse<ResourceCategory>(request.Category, out var category))
        {
            query = query.Where(r => r.Category == category);
        }

        query = query.OrderByDescending(r => r.CreatedAt);

        var page = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResult<ResourceDto>(
            page.Items.Select(r => r.ToDto()).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }
}