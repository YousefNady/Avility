using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.JobApplications.Dtos;
using Avility.Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.JobApplications.Commands.Apply;

public sealed class ApplyToJobCommandHandler : IRequestHandler<ApplyToJobCommand, JobApplicationDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTime _dateTime;

    public ApplyToJobCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IDateTime dateTime)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<JobApplicationDto> Handle(ApplyToJobCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        var jobSeeker = await _dbContext.JobSeekers.FirstOrDefaultAsync(js => js.UserId == userId, cancellationToken)
            ?? throw new ValidationException(new[] { new ValidationFailure("Profile", "You must complete your JobSeeker profile before applying.") });

        var posting = await _dbContext.JobPostings.FirstOrDefaultAsync(p => p.Id == request.JobPostingId, cancellationToken)
            ?? throw new NotFoundException("JobPosting", request.JobPostingId);

        if (!posting.CanAcceptApplications(_dateTime.UtcNow))
        {
            throw new ValidationException(new[] { new ValidationFailure("JobPosting", "This job posting is not accepting applications.") });
        }

        var alreadyApplied = await _dbContext.JobApplications
            .AnyAsync(a => a.JobSeekerId == jobSeeker.Id && a.JobPostingId == request.JobPostingId, cancellationToken);
        if (alreadyApplied)
        {
            throw new ValidationException(new[] { new ValidationFailure("JobPosting", "You have already applied to this job posting.") });
        }

        var application = JobApplication.Create(jobSeeker.Id, request.JobPostingId, request.CoverLetter, _dateTime.UtcNow);

        _dbContext.JobApplications.Add(application);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return application.ToDto();
    }
}
