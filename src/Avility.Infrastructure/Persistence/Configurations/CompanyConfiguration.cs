using Avility.Domain.Entities;
using Avility.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Avility.Infrastructure.Persistence.Configurations;

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.HasIndex(c => c.UserId)
            .IsUnique();

        builder.Property(c => c.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .HasMaxLength(4000);

        builder.Property(c => c.Industry)
            .HasMaxLength(150);

        builder.Property(c => c.WebsiteUrl)
            .HasMaxLength(300);

        builder.Property(c => c.LogoUrl)
            .HasMaxLength(500);

        // Enums stored as strings, per the convention already documented
        // on each enum type. Max length sized to the longest member name
        // with no arbitrary padding - a new member exceeding this would
        // fail fast in a migration/test rather than silently truncating.
        builder.Property(c => c.CompanySize)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30); // longest: "TwoHundredOneToFiveHundred" (26)

        builder.Property(c => c.FoundedYear)
            .IsRequired();

        builder.Property(c => c.VerificationStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20); // longest: "Verified"/"Rejected" (8)

        // No DB-level default for VerificationStatus even though it's
        // always "Pending" at creation - Company.Create() always sets it
        // explicitly via the domain constructor, so EF always includes it
        // in the INSERT. A DB default would be redundant here (unlike
        // IsDeleted, which needs one - see ConfigureAuditableEntity).
        builder.OwnsOne(c => c.Location, location =>
        {
            location.Property(l => l.Country)
                .HasColumnName("LocationCountry")
                .IsRequired()
                .HasMaxLength(100);

            location.Property(l => l.Governorate)
                .HasColumnName("LocationGovernorate")
                .IsRequired()
                .HasMaxLength(100);

            location.Property(l => l.City)
                .HasColumnName("LocationCity")
                .IsRequired()
                .HasMaxLength(100);
        });

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Company>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAuditableEntity();
    }
}
