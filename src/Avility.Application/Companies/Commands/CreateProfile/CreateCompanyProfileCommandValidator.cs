using Avility.Domain.Enums;
using FluentValidation;

namespace Avility.Application.Companies.Commands.CreateProfile;

public sealed class CreateCompanyProfileCommandValidator : AbstractValidator<CreateCompanyProfileCommand>
{
    public CreateCompanyProfileCommandValidator()
    {
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CompanySize)
            .NotEmpty()
            .Must(size => Enum.TryParse<CompanySize>(size, out _))
            .WithMessage("CompanySize must be a valid value (e.g. OneToTen, ElevenToFifty, ...).");
        RuleFor(x => x.FoundedYear).InclusiveBetween(1800, DateTime.UtcNow.Year);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Governorate).NotEmpty().MaximumLength(100);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
    }
}
