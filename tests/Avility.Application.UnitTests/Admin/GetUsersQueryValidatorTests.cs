using Avility.Application.Admin.Queries.GetUsers;
using FluentValidation.TestHelper;
using Xunit;

namespace Avility.Application.UnitTests.Admin;

public class GetUsersQueryValidatorTests
{
    private readonly GetUsersQueryValidator _validator = new();

    [Fact]
    public void Valid_Query_HasNoErrors()
    {
        var result = _validator.TestValidate(new GetUsersQuery());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Invalid_Role_HasError()
    {
        var result = _validator.TestValidate(new GetUsersQuery(Role: "SuperAdmin"));
        result.ShouldHaveValidationErrorFor(x => x.Role);
    }
}