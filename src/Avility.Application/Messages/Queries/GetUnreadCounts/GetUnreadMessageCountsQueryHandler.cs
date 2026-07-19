using Avility.Application.Common.Interfaces;
using Avility.Application.Messages.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Messages.Queries.GetUnreadCounts;

public sealed class GetUnreadMessageCountsQueryHandler : IRequestHandler<GetUnreadMessageCountsQuery, IReadOnlyList<ConversationUnreadCountDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public GetUnreadMessageCountsQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<ConversationUnreadCountDto>> Handle(GetUnreadMessageCountsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        var jobSeeker = await _dbContext.JobSeekers.AsNoTracking().FirstOrDefaultAsync(js => js.UserId == userId, cancellationToken);
        var company = await _dbContext.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        List<Guid> myApplicationIds;
        if (jobSeeker is not null)
        {
            myApplicationIds = await _dbContext.JobApplications.AsNoTracking()
                .Where(a => a.JobSeekerId == jobSeeker.Id)
                .Select(a => a.Id)
                .ToListAsync(cancellationToken);
        }
        else if (company is not null)
        {
            var myPostingIds = _dbContext.JobPostings.AsNoTracking().Where(p => p.CompanyId == company.Id).Select(p => p.Id);
            myApplicationIds = await _dbContext.JobApplications.AsNoTracking()
                .Where(a => myPostingIds.Contains(a.JobPostingId))
                .Select(a => a.Id)
                .ToListAsync(cancellationToken);
        }
        else
        {
            return Array.Empty<ConversationUnreadCountDto>();
        }

        if (myApplicationIds.Count == 0)
        {
            return Array.Empty<ConversationUnreadCountDto>();
        }

        var unreadCounts = await _dbContext.Messages.AsNoTracking()
            .Where(m => myApplicationIds.Contains(m.JobApplicationId) && m.SenderUserId != userId && !m.IsRead)
            .GroupBy(m => m.JobApplicationId)
            .Select(g => new ConversationUnreadCountDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        return unreadCounts;
    }
}