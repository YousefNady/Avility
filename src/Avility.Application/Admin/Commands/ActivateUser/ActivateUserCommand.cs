using MediatR;

namespace Avility.Application.Admin.Commands.ActivateUser;

public sealed record ActivateUserCommand(Guid UserId) : IRequest;
