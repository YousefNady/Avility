using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Common.Services;

public sealed class JobApplicationAccessGuard : IJobApplicationAccessGuard
{
    private readonly IApplicationDbContext _dbContext;

    public JobApplicationAccessGuard(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureParticipantAsync(Guid jobApplicationId, Guid userId, CancellationToken cancellationToken)
    {
        var application = await _dbContext.JobApplications.FirstOrDefaultAsync(a => a.Id == jobApplicationId, cancellationToken)
                          ?? throw new NotFoundException("JobApplication", jobApplicationId);

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