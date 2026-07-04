namespace Avility.Domain.Common;

/// <summary>
/// Base class for all domain entities. Provides identity-based equality,
/// which is the correct equality semantics for entities (as opposed to
/// value objects, which compare by value - see ValueObjects/).
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }

    /// <summary>
    /// Generates a new client-side identity. Entities are always created
    /// fully-formed with an Id, so they can be referenced (e.g. returned
    /// in a command response, or linked to another new entity) before
    /// SaveChanges has run.
    /// </summary>
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Used when reconstituting an entity with a known Id (e.g. in tests,
    /// or when the Id is supplied by a caller).
    /// </summary>
    protected BaseEntity(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Entity Id cannot be empty.", nameof(id));
        }

        Id = id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        return Id == other.Id;
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(BaseEntity? left, BaseEntity? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(BaseEntity? left, BaseEntity? right) => !(left == right);
}
