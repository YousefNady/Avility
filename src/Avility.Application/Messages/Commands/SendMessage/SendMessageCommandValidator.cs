using FluentValidation;

namespace Avility.Application.Messages.Commands.SendMessage;

public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.JobApplicationId).NotEmpty();
        RuleFor(x => x.Body).NotEmpty().MaximumLength(2000);
    }
}