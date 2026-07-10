using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Extensions;
using Avility.Application.Common.Interfaces;
using Avility.Application.Common.Models;
using Avility.Application.Messages.Dtos;
using Avility.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Messages.Queries.GetThread;

public sealed class GetThreadQueryHandler : IRequestHandler<GetThreadQuery, PagedResult<MessageDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public GetThreadQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<MessageDto>> Handle(GetThreadQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        var application = await _dbContext.JobApplications.FirstOrDefaultAsync(a => a.Id == request.JobApplicationId, cancellationToken)
            ?? throw new NotFoundException("JobApplication", request.JobApplicationId);

        await EnsureParticipantAsync(application, userId, cancellationToken);

        // Chronological, oldest-first - a chat thread reads top-to-bottom,
        // unlike the "newest first" convention used by browsing lists
        // elsewhere (job postings, applicants).
        var query = _dbContext.Messages
            .AsNoTracking()
            .Where(m => m.JobApplicationId == request.JobApplicationId)
            .OrderBy(m => m.CreatedAt);

        var page = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResult<MessageDto>(
            page.Items.Select(m => m.ToDto()).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    private async Task EnsureParticipantAsync(JobApplication application, Guid userId, CancellationToken cancellationToken)
    {
        var jobSeeker = await _dbContext.JobSeekers.FirstOrDefaultAsync(js => js.Id == application.JobSeekerId, cancellationToken);
        if (jobSeeker?.UserId == userId)
        {
            return;
        }

        var posting = await _dbContext.JobPostings.FirstOrDefaultAsync(p => p.Id == application.JobPostingId, cancellationToken);
        var company = posting is null ? null : await _dbContext.Companies.FirstOrDefaultAsync(c => c.Id == posting.CompanyId, cancellationToken);
        if (company?.UserId == userId)
        {
            return;
        }

        throw new ForbiddenAccessException();
    }
}