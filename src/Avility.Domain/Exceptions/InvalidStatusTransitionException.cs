namespace Avility.Domain.Exceptions;

/// <summary>
/// Raised when an entity's state machine is asked to make a transition
/// that isn't allowed from its current status (e.g. accepting a
/// JobApplication that has already been withdrawn, or publishing a
/// JobPosting that is already closed).
/// </summary>
public sealed class InvalidStatusTransitionException : DomainException
{
    public InvalidStatusTransitionException(string entityName, string currentStatus, string attemptedStatus)
        : base($"Cannot transition {entityName} from '{currentStatus}' to '{attemptedStatus}'.")
    {
    }
}
