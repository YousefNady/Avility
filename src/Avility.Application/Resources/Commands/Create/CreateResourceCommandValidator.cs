using Avility.Domain.Enums;
using FluentValidation;

namespace Avility.Application.Resources.Commands.Create;

public sealed class CreateResourceCommandValidator : AbstractValidator<CreateResourceCommand>
{
    public CreateResourceCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Url)
            .NotEmpty()
            .MaximumLength(2048)
            .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("Url must be a valid absolute URL.");
        RuleFor(x => x.Category)
            .Must(v => Enum.TryParse<ResourceCategory>(v, out _))
            .WithMessage("Category must be a valid value.");
    }
}