using FluentValidation;

namespace Avility.Application.Companies.Commands.UploadLogo;

public sealed class UploadCompanyLogoCommandValidator : AbstractValidator<UploadCompanyLogoCommand>
{
    private static readonly string[] AllowedContentTypes =
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private const long MaxFileSizeBytes = 2 * 1024 * 1024;

    public UploadCompanyLogoCommandValidator()
    {
        RuleFor(x => x.ContentType)
            .Must(ct => AllowedContentTypes.Contains(ct))
            .WithMessage("Logo must be a JPEG, PNG, or WEBP image.");

        RuleFor(x => x.ContentLength)
            .GreaterThan(0).WithMessage("Logo file is empty.")
            .LessThanOrEqualTo(MaxFileSizeBytes).WithMessage("Logo must not exceed 2MB.");
    }
}