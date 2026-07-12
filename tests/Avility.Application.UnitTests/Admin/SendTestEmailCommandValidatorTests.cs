using Avility.Application.Admin.Commands.SendTestEmail;
using FluentValidation.TestHelper;
using Xunit;

namespace Avility.Application.UnitTests.Admin;

public class SendTestEmailCommandValidatorTests
{
    private readonly SendTestEmailCommandValidator _validator = new();

    [Fact]
    public void Valid_Email_HasNoErrors()
    {
        var result = _validator.TestValidate(new SendTestEmailCommand("someone@example.com"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Invalid_Email_HasError()
    {
        var result = _validator.TestValidate(new SendTestEmailCommand("not-an-email"));
        result.ShouldHaveValidationErrorFor(x => x.ToEmail);
    }
}