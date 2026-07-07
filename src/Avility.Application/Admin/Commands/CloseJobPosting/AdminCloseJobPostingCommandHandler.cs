using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Admin.Commands.CloseJobPosting;

public sealed class AdminCloseJobPostingCommandHandler : IRequestHandler<AdminCloseJobPostingCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTime _dateTime;

    public AdminCloseJobPostingCommandHandler(IApplicationDbContext dbContext, IDateTime dateTime)
    {
        _dbContext = dbContext;
        _dateTime = dateTime;
    }

    public async Task Handle(AdminCloseJobPostingCommand request, CancellationToken cancellationToken)
    {
        var posting = await _dbContext.JobPostings.FirstOrDefaultAsync(p => p.Id == request.JobPostingId, cancellationToken)
            ?? throw new NotFoundException("JobPosting", request.JobPostingId);

        posting.Close(_dateTime.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
