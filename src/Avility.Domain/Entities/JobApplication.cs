using Avility.Domain.Common;
using Avility.Domain.Enums;
using Avility.Domain.Exceptions;

namespace Avility.Domain.Entities;

public sealed class JobApplication : AuditableEntity
{
    public Guid JobSeekerId { get; private set; }
    public Guid JobPostingId { get; private set; }
    public ApplicationStatus Status { get; private set; }
    public string? CoverLetter { get; private set; }
    public DateTime AppliedAt { get; private set; }

    private JobApplication()
    {
    }

    private JobApplication(Guid jobSeekerId, Guid jobPostingId, string? coverLetter, DateTime appliedAt)
    {
        JobSeekerId = jobSeekerId;
        JobPostingId = jobPostingId;
        CoverLetter = coverLetter;
        AppliedAt = appliedAt;
        Status = ApplicationStatus.Applied;
    }

    /// <summary>
    /// Two rules are deliberately NOT enforced here, because they require
    /// information this entity cannot see on its own - the Application
    /// layer must check them before calling Create:
    ///   1. One application per (JobSeeker, JobPosting) - requires
    ///      querying for an existing row; enforced by a DB unique index
    ///      plus an existence check in the command handler.
    ///   2. No reapplying after withdrawal - same reason; requires
    ///      querying for a prior Withdrawn application for this pair.
    /// A single entity, constructed from nothing, has no way to know
    /// whether siblings already exist - that's inherently a query concern.
    /// </summary>
    public static JobApplication Create(Guid jobSeekerId, Guid jobPostingId, string? coverLetter, DateTime utcNow)
    {
        if (jobSeekerId == Guid.Empty)
        {
            throw new DomainValidationException("JobApplication must be linked to a valid job seeker.");
        }

        if (jobPostingId == Guid.Empty)
        {
            throw new DomainValidationException("JobApplication must be linked to a valid job posting.");
        }

        return new JobApplication(jobSeekerId, jobPostingId, coverLetter?.Trim(), utcNow);
    }

    public void MoveToUnderReview()
    {
        EnsureTransitionAllowed(ApplicationStatus.UnderReview, ApplicationStatus.Applied);
        Status = ApplicationStatus.UnderReview;
    }

    /// <summary>Company action.</summary>
    public void Accept()
    {
        EnsureTransitionAllowed(ApplicationStatus.Accepted, ApplicationStatus.Applied, ApplicationStatus.UnderReview);
        Status = ApplicationStatus.Accepted;
    }

    /// <summary>Company action.</summary>
    public void Reject()
    {
        EnsureTransitionAllowed(ApplicationStatus.Rejected, ApplicationStatus.Applied, ApplicationStatus.UnderReview);
        Status = ApplicationStatus.Rejected;
    }

    /// <summary>
    /// Seeker-initiated only - enforced by authorization in the API/Application
    /// layer, not by this method (the entity itself has no concept of "who is calling").
    /// </summary>
    public void Withdraw()
    {
        EnsureTransitionAllowed(ApplicationStatus.Withdrawn, ApplicationStatus.Applied, ApplicationStatus.UnderReview);
        Status = ApplicationStatus.Withdrawn;
    }

    private void EnsureTransitionAllowed(ApplicationStatus target, params ApplicationStatus[] allowedCurrentStatuses)
    {
        if (!allowedCurrentStatuses.Contains(Status))
        {
            throw new InvalidStatusTransitionException(nameof(JobApplication), Status.ToString(), target.ToString());
        }
    }
}
