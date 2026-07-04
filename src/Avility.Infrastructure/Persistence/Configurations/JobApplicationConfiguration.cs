using Avility.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Avility.Infrastructure.Persistence.Configurations;

public sealed class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.ToTable("JobApplications");

        builder.HasKey(ja => ja.Id);

        builder.Property(ja => ja.JobSeekerId)
            .IsRequired();

        builder.Property(ja => ja.JobPostingId)
            .IsRequired();

        builder.Property(ja => ja.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20); // longest: "UnderReview"/"Withdrawn" (11/9)

        builder.Property(ja => ja.CoverLetter)
            .HasMaxLength(4000);

        builder.Property(ja => ja.AppliedAt)
            .IsRequired();

        // This is the database-level enforcement of the "one application
        // per (JobSeeker, JobPosting)" business rule agreed during domain
        // modeling. JobApplication.Create() itself can't enforce this -
        // see the comment on that method - so it's enforced here as a
        // unique composite index, and the Application-layer command
        // handler must additionally check for an existing row before
        // insert to turn a raw DB constraint violation into a proper
        // domain-level error message.
        builder.HasIndex(ja => new { ja.JobSeekerId, ja.JobPostingId })
            .IsUnique();

        // Supports "show me all applications with status X for this
        // posting" - a query a Company will run frequently.
        builder.HasIndex(ja => ja.Status);

        builder.HasOne<JobSeeker>()
            .WithMany()
            .HasForeignKey(ja => ja.JobSeekerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<JobPosting>()
            .WithMany()
            .HasForeignKey(ja => ja.JobPostingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditableEntity();
    }
}
