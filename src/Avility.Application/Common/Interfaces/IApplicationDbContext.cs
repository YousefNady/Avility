using Avility.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Common.Interfaces;

/// <summary>
/// The Application layer's only dependency on persistence. MediatR command
/// and query handlers depend on this interface, never on the concrete
/// ApplicationDbContext or on any EF Core provider package - so Application
/// has no idea whether the database is SQLite, SQL Server, or anything
/// else. See docs/adr/0001-no-generic-repository.md for why this exists
/// instead of a generic IRepository&lt;T&gt;/IUnitOfWork pair: DbSet&lt;T&gt;
/// combined with SaveChangesAsync already gives handlers everything a
/// repository would, including Include, projection, and AsNoTracking,
/// without an extra layer to work around.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<JobSeeker> JobSeekers { get; }
    DbSet<Company> Companies { get; }
    DbSet<JobPosting> JobPostings { get; }
    DbSet<JobApplication> JobApplications { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
