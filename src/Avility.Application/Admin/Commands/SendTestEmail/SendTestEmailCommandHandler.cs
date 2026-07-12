using Avility.Application.Messages;
using MediatR;

namespace Avility.Application.Admin.Commands.SendTestEmail;

public sealed class SendTestEmailCommandHandler : IRequestHandler<SendTestEmailCommand>
{
    private readonly Avility.Application.Common.Interfaces.IEmailSender _emailSender;

    public SendTestEmailCommandHandler(Avility.Application.Common.Interfaces.IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public Task Handle(SendTestEmailCommand request, CancellationToken cancellationToken) =>
        _emailSender.SendAsync(
            request.ToEmail,
            "Avility SMTP test",
            "If you're reading this, Avility's SMTP configuration is working correctly.",
            cancellationToken);
}