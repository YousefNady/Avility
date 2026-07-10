using MediatR;

namespace Avility.Application.Resources.Commands.Delete;

public sealed record DeleteResourceCommand(Guid ResourceId) : IRequest;