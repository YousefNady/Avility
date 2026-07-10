using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.Messages.Dtos;
using Avility.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Messages.Commands.SendMessage;

public sealed class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public SendMessageCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        var application = await _dbContext.JobApplications.FirstOrDefaultAsync(a => a.Id == request.JobApplicationId, cancellationToken)
            ?? throw new NotFoundException("JobApplication", request.JobApplicationId);

        await EnsureParticipantAsync(application, userId, cancellationToken);

        var message = Message.Create(application.Id, userId, request.Body);

        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return message.ToDto();
    }

    /// <summary>
    /// A JobApplication's two participants are the owning JobSeeker and
    /// the owning Company (via its JobPosting). There's no dedicated
    /// "participants" concept on JobApplication itself, so this resolves
    /// both sides and checks the caller is one of them.
    /// </summary>
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