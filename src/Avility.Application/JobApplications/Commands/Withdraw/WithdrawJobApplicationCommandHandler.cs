using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.JobApplications.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.JobApplications.Commands.Withdraw;

public sealed class WithdrawJobApplicationCommandHandler : IRequestHandler<WithdrawJobApplicationCommand, JobApplicationDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public WithdrawJobApplicationCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<JobApplicationDto> Handle(WithdrawJobApplicationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        var application = await _dbContext.JobApplications.FirstOrDefaultAsync(a => a.Id == request.JobApplicationId, cancellationToken)
            ?? throw new NotFoundException("JobApplication", request.JobApplicationId);

        var owns = await _dbContext.JobSeekers.AnyAsync(js => js.Id == application.JobSeekerId && js.UserId == userId, cancellationToken);
        if (!owns)
        {
            throw new ForbiddenAccessException();
        }

        application.Withdraw();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return application.ToDto();
    }
}
