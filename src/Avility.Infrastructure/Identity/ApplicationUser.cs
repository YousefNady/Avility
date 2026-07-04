using Microsoft.AspNetCore.Identity;

namespace Avility.Infrastructure.Identity;

/// <summary>
/// Deliberately minimal for now - this exists in Step 1 only so
/// ApplicationDbContext's base class (IdentityDbContext&lt;ApplicationUser,
/// ApplicationRole, Guid&gt;) has a stable generic signature. Additional
/// fields (e.g. IsActive) and its EF Core fluent configuration are added
/// in Step 4, per the agreed step ordering.
///
/// Uses Guid as the key type (IdentityUser&lt;Guid&gt;), not the ASP.NET
/// Identity default of string, because every domain entity that references
/// a user - JobSeeker.UserId, Company.UserId, RefreshToken.UserId - is
/// typed as Guid.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
}
