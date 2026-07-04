using Microsoft.AspNetCore.Identity;

namespace Avility.Infrastructure.Identity;

/// <summary>
/// Uses Guid as the key type (IdentityUser&lt;Guid&gt;), not the ASP.NET
/// Identity default of string, because every domain entity that references
/// a user - JobSeeker.UserId, Company.UserId, RefreshToken.UserId - is
/// typed as Guid.
///
/// Three properties are added beyond what IdentityUser already provides
/// (Email, PasswordHash, PhoneNumber, LockoutEnd, TwoFactorEnabled,
/// SecurityStamp, etc.) - all three are account-management concerns, not
/// business data:
///
///   - IsActive: a deliberate admin action (deactivating/banning an
///     account), distinct from IdentityUser's own LockoutEnd, which
///     Identity sets automatically after repeated failed login attempts.
///     Conflating the two would make it impossible to tell, from the data
///     alone, whether an account is locked because of a brute-force
///     attempt or because an admin banned it.
///   - CreatedAt: when the account was registered. Set once at creation
///     (in the registration command handler, not via an interceptor -
///     ApplicationUser is an Identity/Infrastructure type, not a Domain
///     AuditableEntity, so it doesn't participate in that pattern).
///   - LastLoginAt: updated on successful sign-in. Purely a
///     security-monitoring/account-activity concern - useful for an Admin
///     dashboard or for flagging dormant accounts - not duplicated
///     anywhere else in the model.
///
/// No navigation collection to JobSeeker, Company, or RefreshToken is
/// added here, consistent with the FK-only relationship pattern used
/// throughout Domain - those relationships are already fully expressed by
/// the Fluent API configurations in Persistence/Configurations/.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }
}

