using Avility.Application.Common.Interfaces;
using MediatR;

namespace Avility.Application.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailSender _emailSender;

    public ForgotPasswordCommandHandler(IIdentityService identityService, IEmailSender emailSender)
    {
        _identityService = identityService;
        _emailSender = emailSender;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var token = await _identityService.GeneratePasswordResetTokenAsync(request.Email);

        // No branch on "user not found": responding identically either way
        // prevents account enumeration via this endpoint.
        if (token is not null)
        {
            var body = $"""
                        You requested a password reset for your Avility account.

                        Reset token: {token}

                        If you didn't request this, you can safely ignore this email.
                        """;

            await _emailSender.SendAsync(request.Email, "Reset your Avility password", body, cancellationToken);
        }
    }
}