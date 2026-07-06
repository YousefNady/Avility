using FluentValidation;

namespace Avility.Application.JobSeekers.Commands.UpdateProfile;

public sealed class UpdateJobSeekerProfileCommandValidator : AbstractValidator<UpdateJobSeekerProfileCommand>
{
    public UpdateJobSeekerProfileCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.YearsOfExperience).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CurrentJobTitle).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Governorate).NotEmpty().MaximumLength(100);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Headline).MaximumLength(150);
        RuleFor(x => x.Bio).MaximumLength(2000);

        RuleFor(x => x.LinkedInUrl).Must(BeAValidUrl).When(x => !string.IsNullOrWhiteSpace(x.LinkedInUrl))
            .WithMessage("LinkedInUrl must be a valid URL.");
        RuleFor(x => x.GitHubUrl).Must(BeAValidUrl).When(x => !string.IsNullOrWhiteSpace(x.GitHubUrl))
            .WithMessage("GitHubUrl must be a valid URL.");
        RuleFor(x => x.PortfolioUrl).Must(BeAValidUrl).When(x => !string.IsNullOrWhiteSpace(x.PortfolioUrl))
            .WithMessage("PortfolioUrl must be a valid URL.");
    }

    private static bool BeAValidUrl(string? url) => Uri.TryCreate(url, UriKind.Absolute, out _);
}
