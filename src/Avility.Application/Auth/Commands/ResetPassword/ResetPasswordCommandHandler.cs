using FluentValidation;
using FluentValidation.Results;
using Avility.Application.Common.Interfaces;
using MediatR;

namespace Avility.Application.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IIdentityService _identityService;

    public ResetPasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var (succeeded, errors) = await _identityService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);

        if (!succeeded)
        {
            throw new ValidationException(new[] { new ValidationFailure(nameof(request.Token), string.Join(" ", errors)) });
        }
    }
}