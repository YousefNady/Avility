using Avility.Application.Companies.Queries.GetByVerificationStatus;
using FluentValidation.TestHelper;
using Xunit;

namespace Avility.Application.UnitTests.Companies;

public class GetCompaniesByVerificationStatusQueryValidatorTests
{
    private readonly GetCompaniesByVerificationStatusQueryValidator _validator = new();

    [Fact]
    public void Valid_Query_HasNoErrors()
    {
        var result = _validator.TestValidate(new GetCompaniesByVerificationStatusQuery(
            Status: "Pending",
            PageNumber: 1,
            PageSize: 10));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Invalid_Status_HasError()
    {
        var result = _validator.TestValidate(new GetCompaniesByVerificationStatusQuery(
            Status: "NotReal",
            PageNumber: 1,
            PageSize: 10));
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Null_Status_HasNoError()
    {
        var result = _validator.TestValidate(new GetCompaniesByVerificationStatusQuery(
            Status: null,
            PageNumber: 1,
            PageSize: 10));
        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }
}
