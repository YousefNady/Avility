using Avility.Application.Common.Constants;
using FluentValidation;

namespace Avility.Application.Auth.Commands.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(role => role == Roles.JobSeeker || role == Roles.Company)
            .WithMessage($"Role must be either '{Roles.JobSeeker}' or '{Roles.Company}'.");
    }
}
