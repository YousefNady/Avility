using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.Resources.Dtos;
using Avility.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Resources.Commands.Update;

public sealed class UpdateResourceCommandHandler : IRequestHandler<UpdateResourceCommand, ResourceDto>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateResourceCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ResourceDto> Handle(UpdateResourceCommand request, CancellationToken cancellationToken)
    {
        var resource = await _dbContext.Resources.FirstOrDefaultAsync(r => r.Id == request.ResourceId, cancellationToken)
                       ?? throw new NotFoundException("Resource", request.ResourceId);

        var category = Enum.Parse<ResourceCategory>(request.Category);
        resource.Update(request.Title, request.Description, request.Url, category);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return resource.ToDto();
    }
}