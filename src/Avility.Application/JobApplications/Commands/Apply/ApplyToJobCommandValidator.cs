using FluentValidation;

namespace Avility.Application.JobApplications.Commands.Apply;

public sealed class ApplyToJobCommandValidator : AbstractValidator<ApplyToJobCommand>
{
    public ApplyToJobCommandValidator()
    {
        RuleFor(x => x.JobPostingId).NotEmpty();
        RuleFor(x => x.CoverLetter).MaximumLength(4000);
    }
}
