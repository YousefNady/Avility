namespace Avility.Domain.Enums;

/// <summary>
/// A fixed enum rather than a free-form ISO currency string, matching the
/// pattern used for the other enums (stored as strings in the DB, safe to
/// extend). Scoped to Egypt/MENA plus a couple of major currencies for the
/// MVP target market - extend this list as the platform expands.
/// </summary>
public enum Currency
{
    EGP,
    USD,
    EUR,
    SAR,
    AED,
    KWD,
    QAR
}
