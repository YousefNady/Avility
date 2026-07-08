using Avility.Application.JobSeekers.Commands.UploadResume;
using FluentValidation.TestHelper;
using Xunit;

namespace Avility.Application.UnitTests.JobSeekers;

public class UploadJobSeekerResumeCommandValidatorTests
{
    private readonly UploadJobSeekerResumeCommandValidator _validator = new();

    private static UploadJobSeekerResumeCommand ValidCommand() =>
        new(Stream.Null, "resume.pdf", "application/pdf", 1024);

    [Fact]
    public void Valid_Command_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DisallowedContentType_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { ContentType = "image/png" });
        result.ShouldHaveValidationErrorFor(x => x.ContentType);
    }

    [Fact]
    public void TooLarge_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { ContentLength = 6 * 1024 * 1024 });
        result.ShouldHaveValidationErrorFor(x => x.ContentLength);
    }

    [Fact]
    public void ZeroLength_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { ContentLength = 0 });
        result.ShouldHaveValidationErrorFor(x => x.ContentLength);
    }
}