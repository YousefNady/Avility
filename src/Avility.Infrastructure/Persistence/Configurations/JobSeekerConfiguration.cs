using Avility.Domain.Entities;
using Avility.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Avility.Infrastructure.Persistence.Configurations;

public sealed class JobSeekerConfiguration : IEntityTypeConfiguration<JobSeeker>
{
    public void Configure(EntityTypeBuilder<JobSeeker> builder)
    {
        // Explicit even though it matches the convention (DbSet name) -
        // documents intent and survives if the DbSet property is ever
        // renamed.
        builder.ToTable("JobSeekers");

        builder.HasKey(js => js.Id);

        builder.Property(js => js.UserId)
            .IsRequired();

        // Enforces the "one JobSeeker profile per user" rule at the
        // database level, not just in application logic.
        builder.HasIndex(js => js.UserId)
            .IsUnique();

        builder.Property(js => js.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(js => js.Headline)
            .HasMaxLength(150);

        builder.Property(js => js.Bio)
            .HasMaxLength(2000);

        builder.Property(js => js.ResumeUrl)
            .HasMaxLength(500);

        builder.Property(js => js.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(js => js.YearsOfExperience)
            .IsRequired();

        builder.Property(js => js.CurrentJobTitle)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(js => js.LinkedInUrl)
            .HasMaxLength(300);

        builder.Property(js => js.GitHubUrl)
            .HasMaxLength(300);

        builder.Property(js => js.PortfolioUrl)
            .HasMaxLength(300);

        // Owned type, table-split into JobSeekers via column prefixes -
        // Location has no identity of its own, so it doesn't get a
        // separate table with its own primary key. Required (not
        // optional) because JobSeeker.Location is a non-nullable
        // property in Domain.
        builder.OwnsOne(js => js.Location, location =>
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

        // FK to Identity's AspNetUsers table. No navigation property on
        // either side (JobSeeker doesn't reference ApplicationUser in
        // Domain, and ApplicationUser doesn't reference JobSeeker) -
        // HasOne<T>() with no navigation expression configures the
        // relationship purely by foreign key. Cascade: deleting the
        // ApplicationUser removes the JobSeeker profile with it.
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<JobSeeker>(js => js.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAuditableEntity();
    }
}
