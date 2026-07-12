using Avility.Domain.Entities;
using Avility.Domain.Enums;
using Avility.Domain.Exceptions;
using Avility.Domain.ValueObjects;
using Xunit;

namespace Avility.Domain.UnitTests.Entities;

public class CompanyTests
{
    private static Location ValidLocation() => Location.Create("Egypt", "Giza", "Giza");

    private static Company ValidCompany() =>
        Company.Create(Guid.NewGuid(), "Acme Inc", CompanySize.OneToTen, 2020, ValidLocation());
    
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
    
    [Fact]
    public void SetLogo_WithValidKey_SetsStorageKey()
    {
        var company = ValidCompany();
        company.SetLogo("abc123.png");

        Assert.Equal("abc123.png", company.LogoStorageKey);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void SetLogo_WithInvalidKey_Throws(string key)
    {
        var company = ValidCompany();
        Assert.Throws<DomainValidationException>(() => company.SetLogo(key));
    }

    [Fact]
    public void RemoveLogo_ClearsStorageKey()
    {
        var company = ValidCompany();
        company.SetLogo("abc123.png");
        company.RemoveLogo();

        Assert.Null(company.LogoStorageKey);
    }

    [Fact]
    public void RemoveLogo_WhenNoneSet_IsNoOp()
    {
        var company = ValidCompany();
        company.RemoveLogo();

        Assert.Null(company.LogoStorageKey);
    }
}
