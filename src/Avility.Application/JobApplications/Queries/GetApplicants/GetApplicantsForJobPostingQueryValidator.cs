using Avility.Domain.Enums;
using FluentValidation;

namespace Avility.Application.JobApplications.Queries.GetApplicants;

public sealed class GetApplicantsForJobPostingQueryValidator : AbstractValidator<GetApplicantsForJobPostingQuery>
{
    public GetApplicantsForJobPostingQueryValidator()
    {
        RuleFor(x => x.JobPostingId).NotEmpty();
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
        RuleFor(x => x.Status)
            .Must(v => Enum.TryParse<ApplicationStatus>(v, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage("Status must be a valid value.");
    }
}
