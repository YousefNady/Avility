using Avility.Application.Resources.Dtos;
using MediatR;

namespace Avility.Application.Resources.Commands.Create;

public sealed record CreateResourceCommand(
    string Title,
    string Description,
    string Url,
    string Category) : IRequest<ResourceDto>;