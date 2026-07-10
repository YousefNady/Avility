using Avility.Domain.Common;
using Avility.Domain.Exceptions;
using Avility.Domain.ValueObjects;
using Avility.Domain.Enums;

namespace Avility.Domain.Entities;

public sealed class JobSeeker : AuditableEntity
{
    /// <summary>
    /// FK to ApplicationUser.Id. Deliberately a plain Guid, not a
    /// navigation property to an Identity type - Domain has no reference
    /// to ASP.NET Identity at all. See docs/adr/ for the reasoning.
    /// </summary>
    public Guid UserId { get; private set; }

    public string FullName { get; private set; } = null!;
    public string? Headline { get; private set; }
    public string? Bio { get; private set; }
    public string? ResumeUrl { get; private set; }
    public string PhoneNumber { get; private set; } = null!;
    public int YearsOfExperience { get; private set; }
    public string CurrentJobTitle { get; private set; } = null!;
    public string? LinkedInUrl { get; private set; }
    public string? GitHubUrl { get; private set; }
    public string? PortfolioUrl { get; private set; }
    public Location Location { get; private set; } = null!;
    public IReadOnlyList<DisabilityCategory> DisabilityCategories { get; private set; } = Array.Empty<DisabilityCategory>();
    public string? AccommodationNotes { get; private set; }

    /// <summary>EF Core requires a parameterless constructor for materialization.</summary>
    private JobSeeker()
    {
    }

    private JobSeeker(
        Guid userId,
        string fullName,
        string phoneNumber,
        int yearsOfExperience,
        string currentJobTitle,
        Location location)
    {
        UserId = userId;
        FullName = fullName;
        PhoneNumber = phoneNumber;
        YearsOfExperience = yearsOfExperience;
        CurrentJobTitle = currentJobTitle;
        Location = location;
    }

    public static JobSeeker Create(
        Guid userId,
        string fullName,
        string phoneNumber,
        int yearsOfExperience,
        string currentJobTitle,
        Location location)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainValidationException("JobSeeker must be linked to a valid user.");
        }

        EnsureValidProfile(fullName, phoneNumber, yearsOfExperience, currentJobTitle, location);

        return new JobSeeker(userId, fullName.Trim(), phoneNumber.Trim(), yearsOfExperience, currentJobTitle.Trim(), location);
    }

    public void UpdateProfile(
        string fullName,
        string? headline,
        string? bio,
        string phoneNumber,
        int yearsOfExperience,
        string currentJobTitle,
        Location location,
        string? linkedInUrl,
        string? gitHubUrl,
        string? portfolioUrl)
    {
        EnsureValidProfile(fullName, phoneNumber, yearsOfExperience, currentJobTitle, location);

        FullName = fullName.Trim();
        Headline = headline?.Trim();
        Bio = bio?.Trim();
        PhoneNumber = phoneNumber.Trim();
        YearsOfExperience = yearsOfExperience;
        CurrentJobTitle = currentJobTitle.Trim();
        Location = location;
        LinkedInUrl = linkedInUrl?.Trim();
        GitHubUrl = gitHubUrl?.Trim();
        PortfolioUrl = portfolioUrl?.Trim();
    }

    public void SetResumeUrl(string resumeUrl)
    {
        if (string.IsNullOrWhiteSpace(resumeUrl))
        {
            throw new DomainValidationException("Resume URL cannot be empty.");
        }

        ResumeUrl = resumeUrl.Trim();
    }
    
    /// <summary>
    /// Self-disclosed and optional, kept separate from UpdateProfile so a
    /// JobSeeker can change this without resubmitting every other field.
    /// No invariant beyond de-duplication - there's no wrong combination
    /// of disability categories to disclose.
    /// </summary>
    public void UpdateAccessibilityInfo(IReadOnlyList<DisabilityCategory>? disabilityCategories, string? accommodationNotes)
    {
        DisabilityCategories = disabilityCategories?.Distinct().ToArray() ?? Array.Empty<DisabilityCategory>();
        AccommodationNotes = accommodationNotes?.Trim();
    }

    /// <summary>
    /// Shared by Create and UpdateProfile so the same rules apply whether
    /// a JobSeeker is being created or edited, without duplicating checks.
    /// </summary>
    private static void EnsureValidProfile(
        string fullName,
        string phoneNumber,
        int yearsOfExperience,
        string currentJobTitle,
        Location location)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainValidationException("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new DomainValidationException("Phone number is required.");
        }

        if (yearsOfExperience < 0)
        {
            throw new DomainValidationException("Years of experience cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currentJobTitle))
        {
            throw new DomainValidationException("Current job title is required.");
        }

        ArgumentNullException.ThrowIfNull(location);
    }
}
