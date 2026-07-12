using Microsoft.AspNetCore.Identity;

namespace Avility.Infrastructure.Identity;

/// <summary>
/// Guid-keyed to match ApplicationUser. Kept as a distinct class (rather
/// than using IdentityRole&lt;Guid&gt; directly) so role-specific behavior
/// or fields can be added later without a breaking change to the
/// DbContext's generic signature.
///
/// Only one property is added: Description, a human-readable explanation
/// of what the role grants, for a future Admin role-management screen.
/// No other role-specific fields are added - the three MVP roles
/// (JobSeeker, Company, Admin) are static reference data, seeded once
/// (see ApplicationRoleConfiguration), and policy-based authorization
/// (agreed during initial design) is expressed as
/// IAuthorizationRequirement/handler classes in the API layer later, not
/// as additional columns on this table. Adding capability flags or
/// permission bitmasks here now would be speculative - there's no current
/// requirement for roles beyond these three fixed ones.
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
}