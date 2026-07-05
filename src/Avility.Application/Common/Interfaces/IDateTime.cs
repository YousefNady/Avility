namespace Avility.Application.Common.Interfaces;

/// <summary>
/// Lets handlers (and the audit interceptor) get the current time without
/// calling DateTime.UtcNow directly, keeping them deterministic and
/// testable. Mirrors the same reasoning documented on Domain's
/// AuditableEntity for why the clock isn't read inline.
/// </summary>
public interface IDateTime
{
    DateTime UtcNow { get; }
}
