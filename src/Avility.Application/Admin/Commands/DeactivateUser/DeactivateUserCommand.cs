using MediatR;

namespace Avility.Application.Admin.Commands.DeactivateUser;

public sealed record DeactivateUserCommand(Guid UserId) : IRequest;
