using Avility.Domain.Entities;
using Avility.Domain.Enums;
using Avility.Domain.Exceptions;
using Xunit;

namespace Avility.Domain.UnitTests.Entities;

public class ResourceTests
{
    private static Resource ValidResource() =>
        Resource.Create("Resume Writing 101", "A guide to writing an accessible resume.", "https://example.com/resume-101", ResourceCategory.ResumeWriting);

    [Fact]
    public void Create_WithValidData_Succeeds()
    {
        var resource = ValidResource();
        Assert.Equal(ResourceCategory.ResumeWriting, resource.Category);
    }

    [Fact]
    public void Create_WithInvalidUrl_Throws()
    {
        Assert.Throws<DomainValidationException>(() =>
            Resource.Create("Title", "Description", "not-a-url", ResourceCategory.Other));
    }

    [Fact]
    public void Update_ChangesDetails()
    {
        var resource = ValidResource();

        resource.Update("New Title", "New description", "https://example.com/new", ResourceCategory.CareerAdvice);

        Assert.Equal("New Title", resource.Title);
        Assert.Equal(ResourceCategory.CareerAdvice, resource.Category);
    }
}