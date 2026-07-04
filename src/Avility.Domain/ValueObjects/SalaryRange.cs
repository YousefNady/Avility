using Avility.Domain.Enums;
using Avility.Domain.Exceptions;

namespace Avility.Domain.ValueObjects;

/// <summary>
/// Bundles SalaryMin, SalaryMax, and Currency - three fields that are
/// always used together and share a real invariant (Min cannot exceed
/// Max). Modeling them as a value object lets that invariant be enforced
/// once, at construction, instead of being re-checked by every command
/// handler that touches a JobPosting's salary. Nullable on JobPosting,
/// since disclosing a salary range is optional.
/// </summary>
public sealed record SalaryRange
{
    public decimal Min { get; }
    public decimal Max { get; }
    public Currency Currency { get; }

    private SalaryRange(decimal min, decimal max, Currency currency)
    {
        Min = min;
        Max = max;
        Currency = currency;
    }

    public static SalaryRange Create(decimal min, decimal max, Currency currency)
    {
        if (min < 0)
        {
            throw new DomainValidationException("Minimum salary cannot be negative.");
        }

        if (max < 0)
        {
            throw new DomainValidationException("Maximum salary cannot be negative.");
        }

        if (min > max)
        {
            throw new DomainValidationException("Minimum salary cannot exceed maximum salary.");
        }

        return new SalaryRange(min, max, currency);
    }
}
