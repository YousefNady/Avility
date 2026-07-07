using Avility.Application.JobApplications.Commands.Apply;
using FluentValidation.TestHelper;
using Xunit;

namespace Avility.Application.UnitTests.JobApplications;

public class ApplyToJobCommandValidatorTests
{
    private readonly ApplyToJobCommandValidator _validator = new();

    [Fact]
    public void Valid_Command_HasNoErrors()
    {
        var result = _validator.TestValidate(new ApplyToJobCommand(Guid.NewGuid(), "Cover letter"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_JobPostingId_HasError()
    {
        var result = _validator.TestValidate(new ApplyToJobCommand(Guid.Empty, null));
        result.ShouldHaveValidationErrorFor(x => x.JobPostingId);
    }

    [Fact]
    public void TooLong_CoverLetter_HasError()
    {
        var result = _validator.TestValidate(new ApplyToJobCommand(Guid.NewGuid(), new string('a', 4001)));
        result.ShouldHaveValidationErrorFor(x => x.CoverLetter);
    }
}
