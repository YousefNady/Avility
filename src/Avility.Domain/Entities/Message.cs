using Avility.Domain.Common;
using Avility.Domain.Exceptions;

namespace Avility.Domain.Entities;

/// <summary>
/// A single message within a JobApplication's conversation thread. Simple
/// and immutable once sent - no edit/delete, no read-receipts. Scoped to
/// a JobApplication because that's where a JobSeeker and a Company are
/// already unambiguously linked; there is no freestanding conversation
/// concept independent of an application.
/// </summary>
public sealed class Message : AuditableEntity
{
    public Guid JobApplicationId { get; private set; }

    /// <summary>
    /// FK to ApplicationUser.Id - whichever side (JobSeeker or Company)
    /// actually sent it. Domain doesn't need to know which side this is;
    /// Application resolves that by comparing against the JobApplication's
    /// own participant UserIds.
    /// </summary>
    public Guid SenderUserId { get; private set; }

    public string Body { get; private set; } = null!;
    
    public bool IsRead { get; private set; }
    
    public DateTime? ReadAt { get; private set; }

    private Message()
    {
    }

    private Message(Guid jobApplicationId, Guid senderUserId, string body)
    {
        JobApplicationId = jobApplicationId;
        SenderUserId = senderUserId;
        Body = body;
    }

    public static Message Create(Guid jobApplicationId, Guid senderUserId, string body)
    {
        if (jobApplicationId == Guid.Empty)
        {
            throw new DomainValidationException("Message must be linked to a valid job application.");
        }

        if (senderUserId == Guid.Empty)
        {
            throw new DomainValidationException("Message must have a valid sender.");
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            throw new DomainValidationException("Message body cannot be empty.");
        }

        return new Message(jobApplicationId, senderUserId, body.Trim());
    }
    
    /// <summary>
    /// Idempotent - safe to call on an already-read message (no-op),
    /// since MarkThreadAsReadCommandHandler applies this to every
    /// currently-unread message in a thread without checking state first.
    /// </summary>
    public void MarkAsRead(DateTime readAt)
    {
        if (IsRead)
        {
            return;
        }

        IsRead = true;
        ReadAt = readAt;
    }
}