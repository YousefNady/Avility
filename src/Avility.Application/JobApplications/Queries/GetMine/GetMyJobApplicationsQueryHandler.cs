using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Extensions;
using Avility.Application.Common.Interfaces;
using Avility.Application.Common.Models;
using Avility.Application.JobApplications.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.JobApplications.Queries.GetMine;

public sealed class GetMyJobApplicationsQueryHandler : IRequestHandler<GetMyJobApplicationsQuery, PagedResult<JobApplicationDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public GetMyJobApplicationsQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<JobApplicationDto>> Handle(GetMyJobApplicationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        var jobSeeker = await _dbContext.JobSeekers.FirstOrDefaultAsync(js => js.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("JobSeeker profile", userId);

        var query = _dbContext.JobApplications.AsNoTracking()
            .Where(a => a.JobSeekerId == jobSeeker.Id)
            .OrderByDescending(a => a.AppliedAt);

        var page = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResult<JobApplicationDto>(
            page.Items.Select(a => a.ToDto()).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }
}
