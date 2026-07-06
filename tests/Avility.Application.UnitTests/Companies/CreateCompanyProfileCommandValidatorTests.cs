using Avility.Application.Companies.Commands.CreateProfile;
using FluentValidation.TestHelper;
using Xunit;

namespace Avility.Application.UnitTests.Companies;

public class CreateCompanyProfileCommandValidatorTests
{
    private readonly CreateCompanyProfileCommandValidator _validator = new();

    private static CreateCompanyProfileCommand ValidCommand() =>
        new("Acme Inc", "ElevenToFifty", 2015, "Egypt", "Giza", "Giza");

    [Fact]
    public void Valid_Command_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Invalid_CompanySize_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { CompanySize = "NotReal" });
        result.ShouldHaveValidationErrorFor(x => x.CompanySize);
    }

    [Fact]
    public void FutureFoundedYear_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { FoundedYear = DateTime.UtcNow.Year + 1 });
        result.ShouldHaveValidationErrorFor(x => x.FoundedYear);
    }
}
