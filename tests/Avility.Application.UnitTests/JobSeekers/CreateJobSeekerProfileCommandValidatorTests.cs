using Avility.Application.JobSeekers.Commands.CreateProfile;
using FluentValidation.TestHelper;
using Xunit;

namespace Avility.Application.UnitTests.JobSeekers;

public class CreateJobSeekerProfileCommandValidatorTests
{
    private readonly CreateJobSeekerProfileCommandValidator _validator = new();

    private static CreateJobSeekerProfileCommand ValidCommand() =>
        new("Sara Ahmed", "+201234567890", 3, "Backend Developer", "Egypt", "Giza", "Giza");

    [Fact]
    public void Valid_Command_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_FullName_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { FullName = "" });
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Negative_YearsOfExperience_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { YearsOfExperience = -1 });
        result.ShouldHaveValidationErrorFor(x => x.YearsOfExperience);
    }

    [Fact]
    public void Empty_City_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { City = "" });
        result.ShouldHaveValidationErrorFor(x => x.City);
    }
}
