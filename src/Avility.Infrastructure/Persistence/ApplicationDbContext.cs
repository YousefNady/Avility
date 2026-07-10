using Avility.Application.Common.Interfaces;
using Avility.Domain.Entities;
using Avility.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Avility.Infrastructure.Persistence;

/// <summary>
/// Inherits IdentityDbContext rather than plain DbContext, because Identity
/// needs its own tables (Users, Roles, UserRoles, etc.) alongside our
/// business entities in the same database. This is a foundational decision
/// made now, in Step 1, rather than in Step 4 alongside the rest of the
/// Identity configuration - changing a DbContext's base class after
/// migrations exist is disruptive, whereas the generic type arguments
/// (ApplicationUser, ApplicationRole, Guid) can be decided up front even
/// though those classes are still just stubs today.
///
/// Deliberately kept clean: this class only declares the DbSets and the
/// constructor. No entity is configured here - see Step 2, where every
/// entity gets its own IEntityTypeConfiguration class, applied via
/// ApplyConfigurationsFromAssembly in OnModelCreating.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<JobSeeker> JobSeekers => Set<JobSeeker>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<JobPosting> JobPostings => Set<JobPosting>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Required first: registers Identity's own entity configurations
        // (Users, Roles, UserRoles, UserClaims, etc.).
        base.OnModelCreating(modelBuilder);

        // Picks up every IEntityTypeConfiguration<T> in this assembly
        // (Persistence/Configurations/). Deliberately the only thing this
        // method does beyond the base call - no entity is configured
        // inline here, so this class stays stable as the model grows.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
