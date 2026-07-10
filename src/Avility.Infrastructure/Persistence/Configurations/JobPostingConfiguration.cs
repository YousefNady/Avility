using Avility.Domain.Entities;
using Avility.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Avility.Infrastructure.Persistence.Configurations;

public sealed class JobPostingConfiguration : IEntityTypeConfiguration<JobPosting>
{
    public void Configure(EntityTypeBuilder<JobPosting> builder)
    {
        builder.ToTable("JobPostings");

        builder.HasKey(jp => jp.Id);

        builder.Property(jp => jp.CompanyId)
            .IsRequired();

        builder.Property(jp => jp.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(jp => jp.Description)
            .IsRequired()
            .HasMaxLength(8000);

        builder.Property(jp => jp.Requirements)
            .HasMaxLength(4000);

        builder.Property(jp => jp.EmploymentType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20); // longest: "Internship"/"Temporary" (10/9)

        builder.Property(jp => jp.ExperienceLevel)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20); // longest: "EntryLevel"/"Executive" (10/9)

        builder.Property(jp => jp.IsRemote)
            .IsRequired();

        builder.Property(jp => jp.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20); // longest: "Published" (9)

        builder.Property(jp => jp.ApplicationDeadline);
        builder.Property(jp => jp.PublishedAt);
        builder.Property(jp => jp.ClosedAt);
        
        builder.Property(jp => jp.SupportedDisabilityCategories)
            .HasConversion(EnumListConverter.Create<DisabilityCategory>(), EnumListConverter.Comparer<DisabilityCategory>())
            .HasMaxLength(200)
            .HasColumnName("SupportedDisabilityCategories");

        builder.Property(jp => jp.AccommodationDetails)
            .HasMaxLength(2000);

        // Optional owned type - JobPosting.Location is nullable (a fully
        // remote job may have none). EF Core treats owned references as
        // required by default, so the Navigation(...).IsRequired(false)
        // call below is mandatory here - omitting it would make EF throw
        // at model-build time the first time a posting without a location
        // is saved.
        builder.OwnsOne(jp => jp.Location, location =>
        {
            location.Property(l => l.Country)
                .HasColumnName("LocationCountry")
                .HasMaxLength(100);

            location.Property(l => l.Governorate)
                .HasColumnName("LocationGovernorate")
                .HasMaxLength(100);

            location.Property(l => l.City)
                .HasColumnName("LocationCity")
                .HasMaxLength(100);
        });
        builder.Navigation(jp => jp.Location).IsRequired(false);

        // Optional owned type - salary disclosure is optional. HasPrecision
        // is a no-op on SQLite today (decimals are stored as TEXT to
        // preserve exact precision, regardless of what's declared here),
        // but declaring it documents intent and means zero rework if this
        // project ever targets SQL Server, where precision is enforced.
        builder.OwnsOne(jp => jp.Salary, salary =>
        {
            salary.Property(s => s.Min)
                .HasColumnName("SalaryMin")
                .HasPrecision(12, 2);

            salary.Property(s => s.Max)
                .HasColumnName("SalaryMax")
                .HasPrecision(12, 2);

            salary.Property(s => s.Currency)
                .HasColumnName("SalaryCurrency")
                .HasConversion<string>()
                .HasMaxLength(3); // ISO-style 3-letter codes (EGP, USD, ...)
        });
        builder.Navigation(jp => jp.Salary).IsRequired(false);

        // FK to Company, no navigation property either direction (Domain
        // holds only CompanyId - see the "no navigation collections"
        // decision from Milestone 1). Restrict: a Company with existing
        // job postings can't be deleted (or cascade-deleted via its
        // owning ApplicationUser) without those postings being dealt
        // with first - see CompanyConfiguration's Cascade-from-User FK
        // for why this interaction is intentional.
        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(jp => jp.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Performance index: the primary "browse open jobs" query filters
        // on Status. (CompanyId already gets an index automatically - EF
        // Core creates one on every FK column by convention - so no
        // explicit index is added for it here.)
        builder.HasIndex(jp => jp.Status);

        builder.ConfigureAuditableEntity();
    }
}
