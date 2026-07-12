using Avility.Application.Resources.Dtos;
using MediatR;

namespace Avility.Application.Resources.Commands.Update;

public sealed record UpdateResourceCommand(
    Guid ResourceId,
    string Title,
    string Description,
    string Url,
    string Category) : IRequest<ResourceDto>;