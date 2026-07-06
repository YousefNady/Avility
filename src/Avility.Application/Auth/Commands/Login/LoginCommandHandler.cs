using FluentValidation;
using FluentValidation.Results;
using Avility.Application.Common.Interfaces;
using Avility.Application.Common.Models;
using MediatR;

namespace Avility.Application.Auth.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IIdentityService _identityService;
    private readonly TokenIssuer _tokenIssuer;

    public LoginCommandHandler(IIdentityService identityService, TokenIssuer tokenIssuer)
    {
        _identityService = identityService;
        _tokenIssuer = tokenIssuer;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.ValidateCredentialsAsync(request.Email, request.Password);

        if (result.Status == CredentialValidationStatus.LockedOut)
        {
            throw new ValidationException(new[] { new ValidationFailure("Email", "Account temporarily locked due to multiple failed attempts.") });
        }

        if (result.Status != CredentialValidationStatus.Success)
        {
            throw new ValidationException(new[] { new ValidationFailure("Email", "Invalid email or password.") });
        }

        await _identityService.UpdateLastLoginAsync(result.UserId);

        return await _tokenIssuer.IssueAsync(result.UserId, request.Email, result.Roles, cancellationToken);
    }
}
