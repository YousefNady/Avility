using Avility.Application.Common.Extensions;
using Avility.Application.Common.Interfaces;
using Avility.Application.Common.Models;
using Avility.Application.JobPostings.Dtos;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.JobPostings.Queries.GetMine;

public sealed class GetMyJobPostingsQueryHandler : IRequestHandler<GetMyJobPostingsQuery, PagedResult<JobPostingDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public GetMyJobPostingsQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<JobPostingDto>> Handle(GetMyJobPostingsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        var company = await _dbContext.Companies.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            ?? throw new ValidationException(new[] { new ValidationFailure("Company", "You must complete your company profile first.") });

        var query = _dbContext.JobPostings.AsNoTracking()
            .Where(p => p.CompanyId == company.Id)
            .OrderByDescending(p => p.CreatedAt);

        var page = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResult<JobPostingDto>(
            page.Items.Select(p => p.ToDto()).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }
}
