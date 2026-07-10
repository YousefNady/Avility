using Avility.Application.Messages.Queries.GetThread;
using FluentValidation.TestHelper;
using Xunit;

namespace Avility.Application.UnitTests.Messages;

public class GetThreadQueryValidatorTests
{
    private readonly GetThreadQueryValidator _validator = new();

    [Fact]
    public void Valid_Query_HasNoErrors()
    {
        var result = _validator.TestValidate(new GetThreadQuery(Guid.NewGuid()));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PageSize_TooLarge_HasError()
    {
        var result = _validator.TestValidate(new GetThreadQuery(Guid.NewGuid(), PageSize: 100));
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }
}