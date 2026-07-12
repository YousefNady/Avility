using Avility.Application.Resources.Dtos;
using MediatR;

namespace Avility.Application.Resources.Queries.GetById;

public sealed record GetResourceByIdQuery(Guid ResourceId) : IRequest<ResourceDto>;