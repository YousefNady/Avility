using Avility.Application.Auth.Commands.ForgotPassword;
using FluentValidation.TestHelper;
using Xunit;

namespace Avility.Application.UnitTests.Auth;

public class ForgotPasswordCommandValidatorTests
{
    private readonly ForgotPasswordCommandValidator _validator = new();

    [Fact]
    public void Valid_Command_HasNoErrors()
    {
        var result = _validator.TestValidate(new ForgotPasswordCommand("user@test.com"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_Email_HasError()
    {
        var result = _validator.TestValidate(new ForgotPasswordCommand(""));
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Malformed_Email_HasError()
    {
        var result = _validator.TestValidate(new ForgotPasswordCommand("not-an-email"));
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}