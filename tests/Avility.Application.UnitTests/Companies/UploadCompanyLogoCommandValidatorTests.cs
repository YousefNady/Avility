using Avility.Application.Companies.Commands.UploadLogo;
using FluentValidation.TestHelper;
using Xunit;

namespace Avility.Application.UnitTests.Companies;

public class UploadCompanyLogoCommandValidatorTests
{
    private readonly UploadCompanyLogoCommandValidator _validator = new();

    private static UploadCompanyLogoCommand ValidCommand() =>
        new(Stream.Null, "logo.png", "image/png", 1024);

    [Fact]
    public void Valid_Command_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DisallowedContentType_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { ContentType = "application/pdf" });
        result.ShouldHaveValidationErrorFor(x => x.ContentType);
    }

    [Fact]
    public void TooLarge_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { ContentLength = 3 * 1024 * 1024 });
        result.ShouldHaveValidationErrorFor(x => x.ContentLength);
    }

    [Fact]
    public void ZeroLength_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { ContentLength = 0 });
        result.ShouldHaveValidationErrorFor(x => x.ContentLength);
    }
}