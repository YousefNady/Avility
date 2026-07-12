namespace Avility.Application.Common.Interfaces;

/// <summary>
/// A JobApplication's two participants are the owning JobSeeker and the
/// owning Company (via its JobPosting) - there's no dedicated
/// "participants" concept on JobApplication itself. This resolves both
/// sides and throws if the caller is neither. Extracted from what used to
/// be two near-identical private methods in SendMessageCommandHandler and
/// GetThreadQueryHandler once a third consumer (MessagesHub) needed the
/// same check - one shared implementation now, used by all three.
/// </summary>
public interface IJobApplicationAccessGuard
{
    /// <summary>Throws NotFoundException or ForbiddenAccessException.</summary>
    Task EnsureParticipantAsync(Guid jobApplicationId, Guid userId, CancellationToken cancellationToken);
}