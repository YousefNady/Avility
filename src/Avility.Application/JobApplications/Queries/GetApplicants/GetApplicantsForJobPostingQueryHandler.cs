using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Extensions;
using Avility.Application.Common.Interfaces;
using Avility.Application.Common.Models;
using Avility.Application.JobApplications.Dtos;
using Avility.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.JobApplications.Queries.GetApplicants;

public sealed class GetApplicantsForJobPostingQueryHandler : IRequestHandler<GetApplicantsForJobPostingQuery, PagedResult<JobApplicationDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public GetApplicantsForJobPostingQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<JobApplicationDto>> Handle(GetApplicantsForJobPostingQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        var posting = await _dbContext.JobPostings.FirstOrDefaultAsync(p => p.Id == request.JobPostingId, cancellationToken)
            ?? throw new NotFoundException("JobPosting", request.JobPostingId);

        var owns = await _dbContext.Companies.AnyAsync(c => c.Id == posting.CompanyId && c.UserId == userId, cancellationToken);
        if (!owns)
        {
            throw new ForbiddenAccessException();
        }

        var query = _dbContext.JobApplications.AsNoTracking().Where(a => a.JobPostingId == request.JobPostingId);

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ApplicationStatus>(request.Status, out var status))
        {
            query = query.Where(a => a.Status == status);
        }

        query = query.OrderByDescending(a => a.AppliedAt);

        var page = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResult<JobApplicationDto>(
            page.Items.Select(a => a.ToDto()).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }
}
