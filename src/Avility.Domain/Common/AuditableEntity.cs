namespace Avility.Domain.Common;

/// <summary>
/// Base class for entities that need created/updated timestamps.
///
/// These properties are intentionally never set by domain logic itself -
/// no constructor or method in this codebase calls DateTime.UtcNow to
/// populate them. Doing that would scatter clock reads throughout the
/// domain layer, make entities non-deterministic in unit tests, and give
/// every entity an implicit dependency on wall-clock time.
///
/// Instead, an EF Core SaveChanges interceptor in the Infrastructure layer
/// (added in Milestone 3) inspects the change tracker on every save and
/// stamps CreatedAt/UpdatedAt for any AuditableEntity being added or
/// modified. The private setters below are still reachable by EF Core's
/// change tracker, which uses property metadata rather than the public
/// C# accessor - so encapsulation from application code is preserved.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected AuditableEntity()
        : base()
    {
    }

    protected AuditableEntity(Guid id)
        : base(id)
    {
    }
}
