using Avility.Domain.Common;
using Avility.Domain.Enums;
using Avility.Domain.Exceptions;
using Avility.Domain.ValueObjects;

namespace Avility.Domain.Entities;

public sealed class JobPosting : AuditableEntity
{
    public Guid CompanyId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string? Requirements { get; private set; }
    public EmploymentType EmploymentType { get; private set; }
    public ExperienceLevel ExperienceLevel { get; private set; }
    public bool IsRemote { get; private set; }

    /// <summary>Null when IsRemote is true; required otherwise (see EnsureLocationValidForRemoteFlag).</summary>
    public Location? Location { get; private set; }

    /// <summary>Optional - companies aren't required to disclose a salary range.</summary>
    public SalaryRange? Salary { get; private set; }

    public DateTime? ApplicationDeadline { get; private set; }
    public JobPostingStatus Status { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public IReadOnlyList<DisabilityCategory> SupportedDisabilityCategories { get; private set; } = Array.Empty<DisabilityCategory>();
    public string? AccommodationDetails { get; private set; }

    private JobPosting()
    {
    }

    private JobPosting(
        Guid companyId,
        string title,
        string description,
        EmploymentType employmentType,
        ExperienceLevel experienceLevel,
        bool isRemote,
        Location? location,
        SalaryRange? salary,
        DateTime? applicationDeadline)
    {
        CompanyId = companyId;
        Title = title;
        Description = description;
        EmploymentType = employmentType;
        ExperienceLevel = experienceLevel;
        IsRemote = isRemote;
        Location = location;
        Salary = salary;
        ApplicationDeadline = applicationDeadline;

        // Every posting starts as a Draft. Publishing is a deliberate,
        // separate action - see Publish() below.
        Status = JobPostingStatus.Draft;
    }

    public static JobPosting Create(
        Guid companyId,
        string title,
        string description,
        EmploymentType employmentType,
        ExperienceLevel experienceLevel,
        bool isRemote,
        Location? location,
        SalaryRange? salary,
        DateTime? applicationDeadline)
    {
        if (companyId == Guid.Empty)
        {
            throw new DomainValidationException("JobPosting must be linked to a valid company.");
        }

        EnsureValidDetails(title, description, isRemote, location);

        return new JobPosting(
            companyId,
            title.Trim(),
            description.Trim(),
            employmentType,
            experienceLevel,
            isRemote,
            location,
            salary,
            applicationDeadline);
    }

    public void UpdateDetails(
        string title,
        string description,
        string? requirements,
        EmploymentType employmentType,
        ExperienceLevel experienceLevel,
        bool isRemote,
        Location? location,
        SalaryRange? salary,
        DateTime? applicationDeadline)
    {
        EnsureEditable();
        EnsureValidDetails(title, description, isRemote, location);

        Title = title.Trim();
        Description = description.Trim();
        Requirements = requirements?.Trim();
        EmploymentType = employmentType;
        ExperienceLevel = experienceLevel;
        IsRemote = isRemote;
        Location = location;
        Salary = salary;
        ApplicationDeadline = applicationDeadline;
    }
    
    /// <summary>
    /// Which accommodation categories this posting/workplace supports.
    /// Kept separate from UpdateDetails for the same reason as
    /// JobSeeker's equivalent method. Still gated by EnsureEditable() - a
    /// closed posting shouldn't be edited at all, accommodation info
    /// included.
    /// </summary>
    public void UpdateAccommodations(IReadOnlyList<DisabilityCategory>? supportedDisabilityCategories, string? accommodationDetails)
    {
        EnsureEditable();
        SupportedDisabilityCategories = supportedDisabilityCategories?.Distinct().ToArray() ?? Array.Empty<DisabilityCategory>();
        AccommodationDetails = accommodationDetails?.Trim();
    }

    /// <summary>
    /// Transitions Draft -> Published.
    ///
    /// Deliberately does NOT check the owning Company's verification
    /// status here, even though "only verified companies can publish" is
    /// an agreed business rule. JobPosting and Company are separate
    /// aggregates - JobPosting only enforces invariants about its own
    /// state (a valid Draft-to-Published transition, a deadline that
    /// hasn't already passed). Checking another aggregate's state
    /// requires loading it, which is an orchestration concern that
    /// belongs to the Application-layer command handler, not to this
    /// entity. The handler will call company.CanPublishJobs() before
    /// calling this method.
    /// </summary>
    public void Publish(DateTime utcNow)
    {
        if (Status != JobPostingStatus.Draft)
        {
            throw new InvalidStatusTransitionException(nameof(JobPosting), Status.ToString(), JobPostingStatus.Published.ToString());
        }

        if (ApplicationDeadline is not null && ApplicationDeadline <= utcNow)
        {
            throw new DomainValidationException("Application deadline must be in the future to publish this job.");
        }

        Status = JobPostingStatus.Published;
        PublishedAt = utcNow;
    }

    /// <summary>
    /// Transitions Draft-or-Published -> Closed. A draft can be closed
    /// directly without ever being published, per the agreed design
    /// decision.
    /// </summary>
    public void Close(DateTime utcNow)
    {
        if (Status == JobPostingStatus.Closed)
        {
            throw new InvalidStatusTransitionException(nameof(JobPosting), Status.ToString(), JobPostingStatus.Closed.ToString());
        }

        Status = JobPostingStatus.Closed;
        ClosedAt = utcNow;
    }

    /// <summary>
    /// Used by the Application layer before creating a JobApplication.
    /// </summary>
    public bool CanAcceptApplications(DateTime utcNow) =>
        Status == JobPostingStatus.Published &&
        (ApplicationDeadline is null || ApplicationDeadline > utcNow);

    private void EnsureEditable()
    {
        if (Status == JobPostingStatus.Closed)
        {
            throw new DomainValidationException("A closed job posting cannot be edited.");
        }
    }

    private static void EnsureValidDetails(string title, string description, bool isRemote, Location? location)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainValidationException("Job title is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainValidationException("Job description is required.");
        }

        if (!isRemote && location is null)
        {
            throw new DomainValidationException("Location is required for on-site or hybrid jobs.");
        }
    }
}
