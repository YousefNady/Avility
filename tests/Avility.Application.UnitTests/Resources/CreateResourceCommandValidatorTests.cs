using Avility.Application.Resources.Commands.Create;
using FluentValidation.TestHelper;
using Xunit;

namespace Avility.Application.UnitTests.Resources;

public class CreateResourceCommandValidatorTests
{
    private readonly CreateResourceCommandValidator _validator = new();

    private static CreateResourceCommand ValidCommand() =>
        new("Title", "Description", "https://example.com", "CareerAdvice");

    [Fact]
    public void Valid_Command_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Invalid_Url_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { Url = "not-a-url" });
        result.ShouldHaveValidationErrorFor(x => x.Url);
    }

    [Fact]
    public void Invalid_Category_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { Category = "VideoContent" });
        result.ShouldHaveValidationErrorFor(x => x.Category);
    }
}