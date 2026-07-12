using Avility.Application.Messages.Commands.SendMessage;
using FluentValidation.TestHelper;
using Xunit;

namespace Avility.Application.UnitTests.Messages;

public class SendMessageCommandValidatorTests
{
    private readonly SendMessageCommandValidator _validator = new();

    [Fact]
    public void Valid_Command_HasNoErrors()
    {
        var result = _validator.TestValidate(new SendMessageCommand(Guid.NewGuid(), "Hi there"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_Body_HasError()
    {
        var result = _validator.TestValidate(new SendMessageCommand(Guid.NewGuid(), ""));
        result.ShouldHaveValidationErrorFor(x => x.Body);
    }

    [Fact]
    public void TooLong_Body_HasError()
    {
        var result = _validator.TestValidate(new SendMessageCommand(Guid.NewGuid(), new string('a', 2001)));
        result.ShouldHaveValidationErrorFor(x => x.Body);
    }
}