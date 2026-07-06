using Avility.Domain.Entities;
using Avility.Domain.Exceptions;
using Avility.Domain.ValueObjects;
using Xunit;

namespace Avility.Domain.UnitTests.Entities;

public class JobSeekerTests
{
    private static Location ValidLocation() => Location.Create("Egypt", "Giza", "Giza");

    [Fact]
    public void Create_WithValidData_Succeeds()
    {
        var seeker = JobSeeker.Create(Guid.NewGuid(), "Sara Ahmed", "+201234567890", 3, "Backend Developer", ValidLocation());

        Assert.Equal("Sara Ahmed", seeker.FullName);
        Assert.Equal(3, seeker.YearsOfExperience);
    }

    [Fact]
    public void Create_WithEmptyUserId_Throws()
    {
        Assert.Throws<DomainValidationException>(() =>
            JobSeeker.Create(Guid.Empty, "Sara Ahmed", "+201234567890", 3, "Backend Developer", ValidLocation()));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidFullName_Throws(string fullName)
    {
        Assert.Throws<DomainValidationException>(() =>
            JobSeeker.Create(Guid.NewGuid(), fullName, "+201234567890", 3, "Backend Developer", ValidLocation()));
    }

    [Fact]
    public void Create_WithNegativeYearsOfExperience_Throws()
    {
        Assert.Throws<DomainValidationException>(() =>
            JobSeeker.Create(Guid.NewGuid(), "Sara Ahmed", "+201234567890", -1, "Backend Developer", ValidLocation()));
    }

    [Fact]
    public void UpdateProfile_WithValidData_UpdatesFields()
    {
        var seeker = JobSeeker.Create(Guid.NewGuid(), "Sara Ahmed", "+201234567890", 3, "Backend Developer", ValidLocation());

        seeker.UpdateProfile(
            "Sara A.", "Senior Dev", "Bio", "+201111111111", 5, "Senior Backend Developer",
            ValidLocation(), "https://linkedin.com/in/sara", null, null);

        Assert.Equal("Sara A.", seeker.FullName);
        Assert.Equal(5, seeker.YearsOfExperience);
        Assert.Equal("https://linkedin.com/in/sara", seeker.LinkedInUrl);
    }

    [Fact]
    public void UpdateProfile_WithEmptyPhoneNumber_Throws()
    {
        var seeker = JobSeeker.Create(Guid.NewGuid(), "Sara Ahmed", "+201234567890", 3, "Backend Developer", ValidLocation());

        Assert.Throws<DomainValidationException>(() =>
            seeker.UpdateProfile("Sara A.", null, null, "", 5, "Senior Backend Developer", ValidLocation(), null, null, null));
    }
}
