using Avility.Application.Common.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Avility.Infrastructure.Identity.Configurations;

/// <summary>
/// Seeds the three fixed MVP roles via HasData rather than an imperative
/// "seed on startup" service. HasData requires every value - including
/// Id and ConcurrencyStamp - to be fully deterministic (the same on every
/// model build), which is why the Guids and stamps below are hardcoded
/// literals instead of Guid.NewGuid(): a non-deterministic seed value
/// would make EF Core think the data changed on every migration, adding a
/// pointless no-op migration each time.
/// </summary>
public sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    private static readonly Guid JobSeekerRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid CompanyRoleId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly Guid AdminRoleId = Guid.Parse("00000000-0000-0000-0000-000000000003");

    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.Property(r => r.Description)
            .HasMaxLength(200);

        builder.HasData(
            new ApplicationRole
            {
                Id = JobSeekerRoleId,
                Name = Roles.JobSeeker,
                NormalizedName = Roles.JobSeeker.ToUpperInvariant(),
                ConcurrencyStamp = "00000000-0000-0000-0000-000000000001",
                Description = "A person seeking employment through the platform."
            },
            new ApplicationRole
            {
                Id = CompanyRoleId,
                Name = Roles.Company,
                NormalizedName = Roles.Company.ToUpperInvariant(),
                ConcurrencyStamp = "00000000-0000-0000-0000-000000000002",
                Description = "An employer posting jobs and reviewing applicants."
            },
            new ApplicationRole
            {
                Id = AdminRoleId,
                Name = Roles.Admin,
                NormalizedName = Roles.Admin.ToUpperInvariant(),
                ConcurrencyStamp = "00000000-0000-0000-0000-000000000003",
                Description = "Platform administrator with moderation and verification privileges."
            });
    }
}
