using FluentValidation;
using FluentValidation.Results;
using Avility.Application.Common.Interfaces;
using MediatR;

namespace Avility.Application.Auth.Commands.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IIdentityService _identityService;
    private readonly TokenIssuer _tokenIssuer;

    public RegisterCommandHandler(IIdentityService identityService, TokenIssuer tokenIssuer)
    {
        _identityService = identityService;
        _tokenIssuer = tokenIssuer;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var (succeeded, userId, errors) = await _identityService.CreateUserAsync(request.Email, request.Password, request.Role);

        if (!succeeded)
        {
            throw new ValidationException(new[] { new ValidationFailure(nameof(request.Email), string.Join(" ", errors)) });
        }

        return await _tokenIssuer.IssueAsync(userId, request.Email, new[] { request.Role }, cancellationToken);
    }
}
