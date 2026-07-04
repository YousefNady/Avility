namespace Avility.Domain.Exceptions;

/// <summary>
/// Raised when a value passed into an entity or value object violates a
/// business rule (e.g. an empty required field, a negative salary, a
/// founded year in the future). This is distinct from FluentValidation,
/// which validates request shape at the Application/API boundary -
/// DomainValidationException guards invariants that must hold no matter
/// how the entity is constructed or mutated.
/// </summary>
public sealed class DomainValidationException : DomainException
{
    public DomainValidationException(string message)
        : base(message)
    {
    }
}
