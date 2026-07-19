using Avility.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Avility.Infrastructure.Persistence.Configurations;

public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.JobApplicationId)
            .IsRequired();

        builder.Property(m => m.SenderUserId)
            .IsRequired();

        builder.Property(m => m.Body)
            .IsRequired()
            .HasMaxLength(2000);
        
        builder.Property(m => m.IsRead)
            .IsRequired();

        builder.Property(m => m.ReadAt);

        // Cascade: a JobApplication's message thread is meaningless
        // without the application itself - unlike JobPosting->JobApplication
        // (Restrict), where existing applications should block deleting
        // the posting they belong to.
        builder.HasOne<JobApplication>()
            .WithMany()
            .HasForeignKey(m => m.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Supports the thread's chronological read query efficiently.
        builder.HasIndex(m => new { m.JobApplicationId, m.CreatedAt });

        builder.ConfigureAuditableEntity();
    }
}