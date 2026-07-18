namespace Avility.Application.Common.Models;

public sealed record UserSummaryDto(
    Guid Id,
    string Email,
    IReadOnlyList<string> Roles,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt);