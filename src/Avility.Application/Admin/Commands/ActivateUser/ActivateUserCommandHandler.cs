using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using MediatR;

namespace Avility.Application.Admin.Commands.ActivateUser;

public sealed class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand>
{
    private readonly IIdentityService _identityService;

    public ActivateUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var succeeded = await _identityService.SetUserActiveStatusAsync(request.UserId, isActive: true);
        if (!succeeded)
        {
            throw new NotFoundException("User", request.UserId);
        }
    }
}
