using Avility.Application.Common.Extensions;
using Avility.Application.Common.Interfaces;
using Avility.Application.Common.Models;
using Avility.Application.Companies.Dtos;
using Avility.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Companies.Queries.GetByVerificationStatus;

public sealed class GetCompaniesByVerificationStatusQueryHandler
    : IRequestHandler<GetCompaniesByVerificationStatusQuery, PagedResult<CompanyProfileDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCompaniesByVerificationStatusQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<CompanyProfileDto>> Handle(GetCompaniesByVerificationStatusQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Companies.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<CompanyVerificationStatus>(request.Status, out var status))
        {
            query = query.Where(c => c.VerificationStatus == status);
        }

        query = query.OrderBy(c => c.CreatedAt);

        var page = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResult<CompanyProfileDto>(
            page.Items.Select(c => c.ToDto()).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }
}
