using Avility.Domain.Entities;
using Avility.Domain.Enums;
using Avility.Domain.Exceptions;
using Avility.Domain.ValueObjects;
using Xunit;

namespace Avility.Domain.UnitTests.Entities;

public class CompanyTests
{
    private static Location ValidLocation() => Location.Create("Egypt", "Giza", "Giza");

    [Fact]
    public void Create_WithValidData_Succeeds()
    {
        var company = Company.Create(Guid.NewGuid(), "Acme Inc", CompanySize.ElevenToFifty, 2015, ValidLocation());

        Assert.Equal("Acme Inc", company.CompanyName);
        Assert.Equal(CompanyVerificationStatus.Pending, company.VerificationStatus);
        Assert.False(company.CanPublishJobs());
    }

    [Fact]
    public void Create_WithFutureFoundedYear_Throws()
    {
        Assert.Throws<DomainValidationException>(() =>
            Company.Create(Guid.NewGuid(), "Acme Inc", CompanySize.ElevenToFifty, DateTime.UtcNow.Year + 1, ValidLocation()));
    }

    [Fact]
    public void Verify_SetsStatusAndAllowsPublishing()
    {
        var company = Company.Create(Guid.NewGuid(), "Acme Inc", CompanySize.ElevenToFifty, 2015, ValidLocation());

        company.Verify();

        Assert.Equal(CompanyVerificationStatus.Verified, company.VerificationStatus);
        Assert.True(company.CanPublishJobs());
    }

    [Fact]
    public void Reject_SetsStatusToRejected()
    {
        var company = Company.Create(Guid.NewGuid(), "Acme Inc", CompanySize.ElevenToFifty, 2015, ValidLocation());

        company.Reject();

        Assert.Equal(CompanyVerificationStatus.Rejected, company.VerificationStatus);
        Assert.False(company.CanPublishJobs());
    }
}
