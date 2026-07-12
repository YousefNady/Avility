using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.JobApplications.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.JobApplications.Commands.MoveToUnderReview;

public sealed class MoveApplicationToUnderReviewCommandHandler : IRequestHandler<MoveApplicationToUnderReviewCommand, JobApplicationDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public MoveApplicationToUnderReviewCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<JobApplicationDto> Handle(MoveApplicationToUnderReviewCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        var application = await _dbContext.JobApplications.FirstOrDefaultAsync(a => a.Id == request.JobApplicationId, cancellationToken)
            ?? throw new NotFoundException("JobApplication", request.JobApplicationId);

        var posting = await _dbContext.JobPostings.FirstOrDefaultAsync(p => p.Id == application.JobPostingId, cancellationToken)
            ?? throw new NotFoundException("JobPosting", application.JobPostingId);

        var owns = await _dbContext.Companies.AnyAsync(c => c.Id == posting.CompanyId && c.UserId == userId, cancellationToken);
        if (!owns)
        {
            throw new ForbiddenAccessException();
        }

        application.MoveToUnderReview();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return application.ToDto();
    }
}
