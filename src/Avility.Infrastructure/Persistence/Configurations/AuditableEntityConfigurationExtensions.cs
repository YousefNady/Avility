using Avility.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Avility.Infrastructure.Persistence.Configurations;

/// <summary>
/// Shared by every IEntityTypeConfiguration for an AuditableEntity, so the
/// CreatedAt/UpdatedAt/IsDeleted/DeletedAt mapping and the soft-delete
/// query filter are defined exactly once instead of five times. This is a
/// small, justified abstraction (not a generic base class or repository -
/// just a static helper) because all five callers need byte-for-byte
/// identical behavior here.
/// </summary>
public static class AuditableEntityConfigurationExtensions
{
    public static void ConfigureAuditableEntity<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : AuditableEntity
    {
        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt);

        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.DeletedAt);

        // Soft-delete query filter. This lives here - in Infrastructure -
        // rather than in Domain, for two reasons:
        //   1. Expression<Func<T, bool>> query filters are an EF Core
        //      concept, tied to ModelBuilder. Putting one in Domain would
        //      give the Domain project a reference to EF Core, which
        //      breaks the "Domain has zero package references" rule -
        //      the whole point of Clean Architecture's dependency
        //      direction.
        //   2. Soft-delete is a persistence-technology decision, not a
        //      business rule. The business rule is "a deleted Company
        //      shouldn't show up in normal queries" - HOW that's achieved
        //      (a WHERE clause EF injects automatically vs. every handler
        //      remembering to filter manually) is an infrastructure
        //      concern. This also makes it easy to deliberately bypass
        //      via .IgnoreQueryFilters() for admin/audit scenarios,
        //      exactly where that kind of override belongs.
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
