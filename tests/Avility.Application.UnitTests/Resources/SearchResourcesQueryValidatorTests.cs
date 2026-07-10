using Avility.Application.Resources.Queries.Search;
using FluentValidation.TestHelper;
using Xunit;

namespace Avility.Application.UnitTests.Resources;

public class SearchResourcesQueryValidatorTests
{
    private readonly SearchResourcesQueryValidator _validator = new();

    [Fact]
    public void Null_Category_HasNoError()
    {
        var result = _validator.TestValidate(new SearchResourcesQuery());
        result.ShouldNotHaveValidationErrorFor(x => x.Category);
    }

    [Fact]
    public void Invalid_Category_HasError()
    {
        var result = _validator.TestValidate(new SearchResourcesQuery(Category: "VideoContent"));
        result.ShouldHaveValidationErrorFor(x => x.Category);
    }
}