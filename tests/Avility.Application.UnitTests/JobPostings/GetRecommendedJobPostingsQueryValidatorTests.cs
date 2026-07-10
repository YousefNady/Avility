using Avility.Application.JobPostings.Queries.GetRecommended;
using FluentValidation.TestHelper;
using Xunit;

namespace Avility.Application.UnitTests.JobPostings;

public class GetRecommendedJobPostingsQueryValidatorTests
{
    private readonly GetRecommendedJobPostingsQueryValidator _validator = new();

    [Fact]
    public void Valid_Query_HasNoErrors()
    {
        var result = _validator.TestValidate(new GetRecommendedJobPostingsQuery());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PageSize_TooLarge_HasError()
    {
        var result = _validator.TestValidate(new GetRecommendedJobPostingsQuery(PageSize: 100));
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }
}