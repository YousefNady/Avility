using Avility.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Avility.Infrastructure.Identity.Configurations;

/// <summary>
/// Configures only the three properties ApplicationUser adds beyond
/// IdentityUser&lt;Guid&gt; itself - the base Identity columns (Email,
/// PasswordHash, LockoutEnd, etc.) are already configured by
/// IdentityDbContext's own model-building logic, invoked via base() in
/// ApplicationDbContext.OnModelCreating. Kept in Identity/Configurations/,
/// separate from Persistence/Configurations/, so the folder structure
/// itself reflects the Identity/business-entity separation, not just the
/// namespaces.
/// </summary>
public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.LastLoginAt);
    }
}
