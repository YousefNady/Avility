using Avility.Application.JobPostings.Commands.Create;
using FluentValidation.TestHelper;
using Xunit;

namespace Avility.Application.UnitTests.JobPostings;

public class CreateJobPostingCommandValidatorTests
{
    private readonly CreateJobPostingCommandValidator _validator = new();

    private static CreateJobPostingCommand ValidCommand() => new(
        "Backend Engineer", "Build APIs", null, "FullTime", "MidLevel", false,
        "Egypt", "Giza", "Giza", null, null, null, null);

    [Fact]
    public void Valid_Command_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void OnSite_WithoutCountry_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { Country = null });
        result.ShouldHaveValidationErrorFor(x => x.Country);
    }

    [Fact]
    public void Remote_WithoutLocation_HasNoLocationError()
    {
        var result = _validator.TestValidate(ValidCommand() with { IsRemote = true, Country = null, Governorate = null, City = null });
        result.ShouldNotHaveValidationErrorFor(x => x.Country);
    }

    [Fact]
    public void Invalid_EmploymentType_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { EmploymentType = "NotReal" });
        result.ShouldHaveValidationErrorFor(x => x.EmploymentType);
    }

    [Fact]
    public void PartialSalary_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { SalaryMin = 1000 });
        Assert.False(result.IsValid);
    }
}
