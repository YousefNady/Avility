using Avility.Domain.Entities;
using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.JobApplications.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.JobApplications.Commands.Accept;

public sealed class AcceptJobApplicationCommandHandler : IRequestHandler<AcceptJobApplicationCommand, JobApplicationDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailSender _emailSender;
    private readonly IIdentityService _identityService;

    public AcceptJobApplicationCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUser,
        IEmailSender emailSender,
        IIdentityService identityService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _emailSender = emailSender;
        _identityService = identityService;
    }

    public async Task<JobApplicationDto> Handle(AcceptJobApplicationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        var application = await _dbContext.JobApplications.FirstOrDefaultAsync(a => a.Id == request.JobApplicationId, cancellationToken)
            ?? throw new NotFoundException("JobApplication", request.JobApplicationId);

        var posting = await _dbContext.JobPostings.FirstOrDefaultAsync(p => p.Id == application.JobPostingId, cancellationToken)
            ?? throw new NotFoundException("JobPosting", application.JobPostingId);

        var company = await _dbContext.Companies.FirstOrDefaultAsync(c => c.Id == posting.CompanyId, cancellationToken)
                      ?? throw new NotFoundException("Company", posting.CompanyId);

        if (company.UserId != userId)
        {
            throw new ForbiddenAccessException();
        }

        application.Accept();
        await _dbContext.SaveChangesAsync(cancellationToken);

        await NotifyJobSeekerAsync(application, posting, company, "accepted", cancellationToken);

        return application.ToDto();
    }

    private async Task NotifyJobSeekerAsync(JobApplication application, JobPosting posting, Company company, string outcome, CancellationToken cancellationToken)
    {
        var jobSeeker = await _dbContext.JobSeekers.FirstOrDefaultAsync(js => js.Id == application.JobSeekerId, cancellationToken);
        if (jobSeeker is null)
        {
            return;
        }

        var userInfo = await _identityService.GetUserInfoAsync(jobSeeker.UserId);
        if (userInfo is null)
        {
            return;
        }

        var body = $"""
                    Your application for "{posting.Title}" at {company.CompanyName} has been {outcome}.
                    """;

        await _emailSender.SendAsync(userInfo.Value.Email, $"Your application was {outcome}", body, cancellationToken);
    }
}
    

