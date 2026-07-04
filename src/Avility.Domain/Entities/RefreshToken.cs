using Avility.Domain.Common;
using Avility.Domain.Exceptions;

namespace Avility.Domain.Entities;

/// <summary>
/// Inherits BaseEntity directly rather than AuditableEntity. A refresh
/// token isn't "updated" the way a profile is - it's issued, then later
/// revoked (optionally replaced by a new one during rotation). It has its
/// own CreatedAt for that reason, and no UpdatedAt at all.
/// </summary>
public sealed class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }

    /// <summary>
    /// The raw token value is never stored - only its hash. The API layer
    /// hashes the token before comparing it against this value.
    /// </summary>
    public string TokenHash { get; private set; } = null!;

    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CreatedByIp { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    /// <summary>Set when this token was rotated out in favor of a newer one, forming an audit chain.</summary>
    public string? ReplacedByTokenHash { get; private set; }

    public bool IsRevoked => RevokedAt is not null;

    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAt;

    public bool IsActive(DateTime utcNow) => !IsRevoked && !IsExpired(utcNow);

    private RefreshToken()
    {
    }

    private RefreshToken(Guid userId, string tokenHash, DateTime expiresAt, DateTime createdAt, string? createdByIp)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
        CreatedByIp = createdByIp;
    }

    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        DateTime expiresAt,
        DateTime createdAt,
        string? createdByIp = null)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainValidationException("RefreshToken must be linked to a valid user.");
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new DomainValidationException("Token hash is required.");
        }

        if (expiresAt <= createdAt)
        {
            throw new DomainValidationException("Expiry must be after creation time.");
        }

        return new RefreshToken(userId, tokenHash, expiresAt, createdAt, createdByIp);
    }

    /// <summary>Called during rotation (pass the new token's hash) or plain logout (pass null).</summary>
    public void Revoke(DateTime utcNow, string? replacedByTokenHash = null)
    {
        if (IsRevoked)
        {
            throw new DomainValidationException("Refresh token has already been revoked.");
        }

        RevokedAt = utcNow;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
