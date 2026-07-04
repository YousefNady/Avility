using Microsoft.AspNetCore.Identity;

namespace Avility.Infrastructure.Identity;

/// <summary>
/// Guid-keyed to match ApplicationUser. Kept as a distinct class (rather
/// than using IdentityRole&lt;Guid&gt; directly) so role-specific behavior
/// or fields can be added later without a breaking change to the
/// DbContext's generic signature.
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
}
