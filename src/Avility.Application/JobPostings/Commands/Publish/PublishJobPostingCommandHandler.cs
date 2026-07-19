using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.JobPostings.Dtos;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.JobPostings.Commands.Publish;

public sealed class PublishJobPostingCommandHandler : IRequestHandler<PublishJobPostingCommand, JobPostingDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTime _dateTime;

    public PublishJobPostingCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IDateTime dateTime)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<JobPostingDto> Handle(PublishJobPostingCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        var posting = await _dbContext.JobPostings.FirstOrDefaultAsync(p => p.Id == request.JobPostingId, cancellationToken)
            ?? throw new NotFoundException("JobPosting", request.JobPostingId);

        var company = await _dbContext.Companies.FirstOrDefaultAsync(c => c.Id == posting.CompanyId, cancellationToken)
            ?? throw new NotFoundException("Company", posting.CompanyId);

        if (company.UserId != userId)
        {
            throw new ForbiddenAccessException();
        }

        if (!company.CanPublishJobs())
        {
            throw new ValidationException(new[] { new ValidationFailure("Company", "Your company must be verified before publishing job postings.") });
        }

        posting.Publish(_dateTime.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return posting.ToDto(company);
    }
}
