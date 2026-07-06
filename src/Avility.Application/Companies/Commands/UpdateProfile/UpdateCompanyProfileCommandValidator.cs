using Avility.Domain.Enums;
using FluentValidation;

namespace Avility.Application.Companies.Commands.UpdateProfile;

public sealed class UpdateCompanyProfileCommandValidator : AbstractValidator<UpdateCompanyProfileCommand>
{
    public UpdateCompanyProfileCommandValidator()
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
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.Industry).MaximumLength(150);
        RuleFor(x => x.WebsiteUrl).MaximumLength(300);
        RuleFor(x => x.LogoUrl).MaximumLength(500);
    }
}
