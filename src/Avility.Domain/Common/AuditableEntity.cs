namespace Avility.Domain.Common;

/// <summary>
/// Base class for entities that need created/updated timestamps and
/// soft-delete support.
///
/// CreatedAt/UpdatedAt are intentionally never set by domain logic itself -
/// no constructor or method in this codebase calls DateTime.UtcNow to
/// populate them. Doing that would scatter clock reads throughout the
/// domain layer, make entities non-deterministic in unit tests, and give
/// every entity an implicit dependency on wall-clock time.
///
/// Instead, an EF Core SaveChanges interceptor in the Infrastructure layer
/// inspects the change tracker on every save and stamps CreatedAt/UpdatedAt
/// for any AuditableEntity being added or modified. The private setters
/// below are still reachable by EF Core's change tracker, which uses
/// property metadata rather than the public C# accessor - so encapsulation
/// from application code is preserved.
///
/// IsDeleted/DeletedAt are applied uniformly to every AuditableEntity
/// (rather than only Company/JobPosting as originally scoped) so that a
/// single EF Core query filter convention can apply to all of them
/// consistently, instead of the Persistence layer having to remember which
/// entities opted in. MarkAsDeleted/Restore are on this base class; nothing
/// requires an entity to ever call them.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    protected AuditableEntity()
        : base()
    {
    }

    protected AuditableEntity(Guid id)
        : base(id)
    {
    }

    /// <summary>Idempotent - marking an already-deleted entity as deleted again is a no-op.</summary>
    public void MarkAsDeleted(DateTime utcNow)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAt = utcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
    }
}

