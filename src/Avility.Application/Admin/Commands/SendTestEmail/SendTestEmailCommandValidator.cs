using FluentValidation;

namespace Avility.Application.Admin.Commands.SendTestEmail;

public sealed class SendTestEmailCommandValidator : AbstractValidator<SendTestEmailCommand>
{
    public SendTestEmailCommandValidator()
    {
        RuleFor(x => x.ToEmail).NotEmpty().EmailAddress();
    }
}