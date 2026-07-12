using Avility.Domain.Entities;
using Avility.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Avility.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.UserId)
            .IsRequired();

        builder.Property(rt => rt.TokenHash)
            .IsRequired()
            .HasMaxLength(200); // SHA-256 hex digest is 64 chars; 200 leaves headroom for other hash algorithms

        // Prevents two rows ever sharing the same hash - both a sanity
        // check and a defense against a hash-collision-based lookup bug
        // silently matching the wrong token.
        builder.HasIndex(rt => rt.TokenHash)
            .IsUnique();

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();

        builder.Property(rt => rt.CreatedAt)
            .IsRequired();

        builder.Property(rt => rt.CreatedByIp)
            .HasMaxLength(45); // max length of an IPv6 address in string form

        builder.Property(rt => rt.RevokedAt);

        builder.Property(rt => rt.ReplacedByTokenHash)
            .HasMaxLength(200);

        // Cascade: a deleted user's refresh tokens are meaningless and
        // should go with them - unlike JobPosting/JobApplication FKs,
        // there's no "existing data" reason to block this delete.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // No ConfigureAuditableEntity() call - RefreshToken inherits
        // BaseEntity, not AuditableEntity (see the type itself for why:
        // tokens are issued and later revoked, never "soft-deleted" in
        // the same sense as a profile or job posting).
    }
}
