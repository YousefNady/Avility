using Avility.Domain.Common;
using Avility.Domain.Enums;
using Avility.Domain.Exceptions;
using Avility.Domain.ValueObjects;

namespace Avility.Domain.Entities;

public sealed class Company : AuditableEntity
{
    public Guid UserId { get; private set; }
    public string CompanyName { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? Industry { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public string? LogoUrl { get; private set; }
    public CompanySize CompanySize { get; private set; }
    public int FoundedYear { get; private set; }
    public CompanyVerificationStatus VerificationStatus { get; private set; }
    public Location Location { get; private set; } = null!;

    private Company()
    {
    }

    private Company(Guid userId, string companyName, CompanySize companySize, int foundedYear, Location location)
    {
        UserId = userId;
        CompanyName = companyName;
        CompanySize = companySize;
        FoundedYear = foundedYear;
        Location = location;

        // Every company starts unverified. Verification is a deliberate,
        // separate Admin action - see Verify() below.
        VerificationStatus = CompanyVerificationStatus.Pending;
    }

    public static Company Create(
        Guid userId,
        string companyName,
        CompanySize companySize,
        int foundedYear,
        Location location)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainValidationException("Company must be linked to a valid user.");
        }

        EnsureValidProfile(companyName, foundedYear, location);

        return new Company(userId, companyName.Trim(), companySize, foundedYear, location);
    }

    public void UpdateProfile(
        string companyName,
        string? description,
        string? industry,
        string? websiteUrl,
        string? logoUrl,
        CompanySize companySize,
        int foundedYear,
        Location location)
    {
        EnsureValidProfile(companyName, foundedYear, location);

        CompanyName = companyName.Trim();
        Description = description?.Trim();
        Industry = industry?.Trim();
        WebsiteUrl = websiteUrl?.Trim();
        LogoUrl = logoUrl?.Trim();
        CompanySize = companySize;
        FoundedYear = foundedYear;
        Location = location;
    }

    /// <summary>Admin action. Idempotent - re-verifying an already-verified company is a no-op.</summary>
    public void Verify()
    {
        VerificationStatus = CompanyVerificationStatus.Verified;
    }

    /// <summary>Admin action.</summary>
    public void Reject()
    {
        VerificationStatus = CompanyVerificationStatus.Rejected;
    }

    /// <summary>
    /// Exposed so the Application layer can check this rule before
    /// publishing a JobPosting. Deliberately not enforced inside
    /// JobPosting.Publish() itself - see the note on that method for why.
    /// </summary>
    public bool CanPublishJobs() => VerificationStatus == CompanyVerificationStatus.Verified;

    private static void EnsureValidProfile(string companyName, int foundedYear, Location location)
    {
        if (string.IsNullOrWhiteSpace(companyName))
        {
            throw new DomainValidationException("Company name is required.");
        }

        if (foundedYear < 1800 || foundedYear > DateTime.UtcNow.Year)
        {
            throw new DomainValidationException("Founded year is not valid.");
        }

        ArgumentNullException.ThrowIfNull(location);
    }
}
