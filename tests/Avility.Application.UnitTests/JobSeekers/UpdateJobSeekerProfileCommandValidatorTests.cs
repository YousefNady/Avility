using Avility.Application.JobSeekers.Commands.UpdateProfile;
using FluentValidation.TestHelper;
using Xunit;

namespace Avility.Application.UnitTests.JobSeekers;

public class UpdateJobSeekerProfileCommandValidatorTests
{
    private readonly UpdateJobSeekerProfileCommandValidator _validator = new();

    private static UpdateJobSeekerProfileCommand ValidCommand() => new(
        "Sara Ahmed", "Senior Dev", "Bio", "+201234567890", 5, "Senior Backend Developer",
        "Egypt", "Giza", "Giza", "https://linkedin.com/in/sara", null, null);

    [Fact]
    public void Valid_Command_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Invalid_LinkedInUrl_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { LinkedInUrl = "not-a-url" });
        result.ShouldHaveValidationErrorFor(x => x.LinkedInUrl);
    }

    [Fact]
    public void Empty_FullName_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { FullName = "" });
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }
}
