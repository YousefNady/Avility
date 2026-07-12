using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Avility.Infrastructure.Persistence.Configurations;

/// <summary>
/// Shared conversion for a list-of-enum property stored as a single
/// comma-separated string column - the same "enums as strings" principle
/// used everywhere else in this project, extended to a small collection
/// rather than a single value. Used by JobSeeker.DisabilityCategories and
/// JobPosting.SupportedDisabilityCategories today; reusable for any future
/// enum-list property.
/// </summary>
internal static class EnumListConverter
{
    public static ValueConverter<IReadOnlyList<TEnum>, string> Create<TEnum>() where TEnum : struct, Enum =>
        new(
            v => string.Join(',', v.Select(e => e.ToString())),
            v => string.IsNullOrEmpty(v)
                ? Array.Empty<TEnum>()
                : v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Enum.Parse<TEnum>).ToArray());

    public static ValueComparer<IReadOnlyList<TEnum>> Comparer<TEnum>() where TEnum : struct, Enum =>
        new(
            (a, b) => a!.SequenceEqual(b!),
            v => v.Aggregate(0, (hash, e) => HashCode.Combine(hash, e)),
            v => v.ToList());
}