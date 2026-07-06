using Avility.Domain.Enums;
using FluentValidation;

namespace Avility.Application.JobPostings.Commands.Create;

public sealed class CreateJobPostingCommandValidator : AbstractValidator<CreateJobPostingCommand>
{
    public CreateJobPostingCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(8000);
        RuleFor(x => x.Requirements).MaximumLength(4000);

        RuleFor(x => x.EmploymentType)
            .NotEmpty()
            .Must(v => Enum.TryParse<EmploymentType>(v, out _))
            .WithMessage("EmploymentType must be a valid value.");

        RuleFor(x => x.ExperienceLevel)
            .NotEmpty()
            .Must(v => Enum.TryParse<ExperienceLevel>(v, out _))
            .WithMessage("ExperienceLevel must be a valid value.");

        When(x => !x.IsRemote, () =>
        {
            RuleFor(x => x.Country).NotEmpty();
            RuleFor(x => x.Governorate).NotEmpty();
            RuleFor(x => x.City).NotEmpty();
        });

        RuleFor(x => x.SalaryCurrency)
            .Must(v => Enum.TryParse<Currency>(v, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.SalaryCurrency))
            .WithMessage("SalaryCurrency must be a valid value.");

        RuleFor(x => x)
            .Must(x => (x.SalaryMin is null && x.SalaryMax is null && x.SalaryCurrency is null) ||
                       (x.SalaryMin is not null && x.SalaryMax is not null && x.SalaryCurrency is not null))
            .WithMessage("SalaryMin, SalaryMax, and SalaryCurrency must all be provided together, or all omitted.");
    }
}
