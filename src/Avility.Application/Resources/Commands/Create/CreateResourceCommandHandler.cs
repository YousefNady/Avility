using Avility.Application.Common.Interfaces;
using Avility.Application.Resources.Dtos;
using Avility.Domain.Entities;
using Avility.Domain.Enums;
using MediatR;

namespace Avility.Application.Resources.Commands.Create;

public sealed class CreateResourceCommandHandler : IRequestHandler<CreateResourceCommand, ResourceDto>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateResourceCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ResourceDto> Handle(CreateResourceCommand request, CancellationToken cancellationToken)
    {
        var category = Enum.Parse<ResourceCategory>(request.Category);
        var resource = Resource.Create(request.Title, request.Description, request.Url, category);

        _dbContext.Resources.Add(resource);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return resource.ToDto();
    }
}