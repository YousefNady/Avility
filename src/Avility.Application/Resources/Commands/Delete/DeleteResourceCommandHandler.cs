using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Resources.Commands.Delete;

public sealed class DeleteResourceCommandHandler : IRequestHandler<DeleteResourceCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTime _dateTime;

    public DeleteResourceCommandHandler(IApplicationDbContext dbContext, IDateTime dateTime)
    {
        _dbContext = dbContext;
        _dateTime = dateTime;
    }

    public async Task Handle(DeleteResourceCommand request, CancellationToken cancellationToken)
    {
        var resource = await _dbContext.Resources.FirstOrDefaultAsync(r => r.Id == request.ResourceId, cancellationToken)
                       ?? throw new NotFoundException("Resource", request.ResourceId);

        resource.MarkAsDeleted(_dateTime.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}