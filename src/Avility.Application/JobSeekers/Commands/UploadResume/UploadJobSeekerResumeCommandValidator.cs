using FluentValidation;

namespace Avility.Application.JobSeekers.Commands.UploadResume;

public sealed class UploadJobSeekerResumeCommandValidator : AbstractValidator<UploadJobSeekerResumeCommand>
{
    private static readonly string[] AllowedContentTypes =
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    private const long MaxSizeBytes = 5 * 1024 * 1024;

    public UploadJobSeekerResumeCommandValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);

        RuleFor(x => x.ContentType)
            .Must(ct => AllowedContentTypes.Contains(ct))
            .WithMessage("Resume must be a PDF or Word document.");

        RuleFor(x => x.ContentLength)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxSizeBytes)
            .WithMessage("Resume must not exceed 5MB.");
    }
}