using Avility.Application.JobPostings.Queries.Search;
using FluentValidation.TestHelper;
using Xunit;

namespace Avility.Application.UnitTests.JobPostings;

public class SearchJobPostingsQueryValidatorTests
{
    private readonly SearchJobPostingsQueryValidator _validator = new();

    [Fact]
    public void Valid_Query_HasNoErrors()
    {
        var result = _validator.TestValidate(new SearchJobPostingsQuery(DisabilityCategory: "Visual"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Invalid_DisabilityCategory_HasError()
    {
        var result = _validator.TestValidate(new SearchJobPostingsQuery(DisabilityCategory: "NotARealCategory"));
        result.ShouldHaveValidationErrorFor(x => x.DisabilityCategory);
    }

    [Fact]
    public void Null_DisabilityCategory_HasNoError()
    {
        var result = _validator.TestValidate(new SearchJobPostingsQuery());
        result.ShouldNotHaveValidationErrorFor(x => x.DisabilityCategory);
    }
}