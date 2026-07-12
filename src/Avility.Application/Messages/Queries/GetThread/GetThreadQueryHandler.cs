using Avility.Application.Common.Extensions;
using Avility.Application.Common.Interfaces;
using Avility.Application.Common.Models;
using Avility.Application.Messages.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Messages.Queries.GetThread;

public sealed class GetThreadQueryHandler : IRequestHandler<GetThreadQuery, PagedResult<MessageDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IJobApplicationAccessGuard _accessGuard;

    public GetThreadQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IJobApplicationAccessGuard accessGuard)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _accessGuard = accessGuard;
    }

    public async Task<PagedResult<MessageDto>> Handle(GetThreadQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        await _accessGuard.EnsureParticipantAsync(request.JobApplicationId, userId, cancellationToken);

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
}