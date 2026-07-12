using Avility.Application.Companies.Commands.UpdateProfile;
using FluentValidation.TestHelper;
using Xunit;

namespace Avility.Application.UnitTests.Companies;

public class UpdateCompanyProfileCommandValidatorTests
{
    private readonly UpdateCompanyProfileCommandValidator _validator = new();

    private static UpdateCompanyProfileCommand ValidCommand() => new(
        "Acme Inc", "We build things", "Software", "https://acme.com", null,
        "ElevenToFifty", 2015, "Egypt", "Giza", "Giza");

    [Fact]
    public void Valid_Command_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_CompanyName_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { CompanyName = "" });
        result.ShouldHaveValidationErrorFor(x => x.CompanyName);
    }
}
