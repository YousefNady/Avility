using Avility.Domain.Enums;
using FluentValidation;

namespace Avility.Application.JobSeekers.Commands.CreateProfile;

public sealed class CreateJobSeekerProfileCommandValidator : AbstractValidator<CreateJobSeekerProfileCommand>
{
    public CreateJobSeekerProfileCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.YearsOfExperience).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CurrentJobTitle).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Governorate).NotEmpty().MaximumLength(100);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleForEach(x => x.DisabilityCategories)
                    .Must(v => Enum.TryParse<DisabilityCategory>(v, out _))
                    .WithMessage("Each disability category must be a valid value.");
                RuleFor(x => x.AccommodationNotes).MaximumLength(1000);
    }
}
