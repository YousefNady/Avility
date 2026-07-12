using Avility.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Avility.Infrastructure.Persistence.Configurations;

public sealed class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("Resources");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(r => r.Url)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(r => r.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30); // longest: "MentalHealthAndWellbeing" (24)

        builder.HasIndex(r => r.Category);

        builder.ConfigureAuditableEntity();
    }
}