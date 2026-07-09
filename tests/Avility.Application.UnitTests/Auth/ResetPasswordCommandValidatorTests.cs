using Avility.Application.Auth.Commands.ResetPassword;
using FluentValidation.TestHelper;
using Xunit;

namespace Avility.Application.UnitTests.Auth;

public class ResetPasswordCommandValidatorTests
{
    private readonly ResetPasswordCommandValidator _validator = new();

    private static ResetPasswordCommand ValidCommand() =>
        new("user@test.com", "some-token", "NewPassword123");

    [Fact]
    public void Valid_Command_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_Token_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { Token = "" });
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void Short_NewPassword_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { NewPassword = "short" });
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }
}