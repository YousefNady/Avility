using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.Resources.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Resources.Queries.GetById;

public sealed class GetResourceByIdQueryHandler : IRequestHandler<GetResourceByIdQuery, ResourceDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetResourceByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ResourceDto> Handle(GetResourceByIdQuery request, CancellationToken cancellationToken)
    {
        var resource = await _dbContext.Resources.AsNoTracking().FirstOrDefaultAsync(r => r.Id == request.ResourceId, cancellationToken)
                       ?? throw new NotFoundException("Resource", request.ResourceId);

        return resource.ToDto();
    }
}