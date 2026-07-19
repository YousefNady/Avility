using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.JobPostings.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.JobPostings.Commands.Close;

public sealed class CloseJobPostingCommandHandler : IRequestHandler<CloseJobPostingCommand, JobPostingDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTime _dateTime;

    public CloseJobPostingCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IDateTime dateTime)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<JobPostingDto> Handle(CloseJobPostingCommand request, CancellationToken cancellationToken)
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

        posting.Close(_dateTime.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return posting.ToDto(company);
    }
}
