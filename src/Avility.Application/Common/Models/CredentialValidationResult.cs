namespace Avility.Application.Common.Models;

public enum CredentialValidationStatus
{
    Success,
    InvalidCredentials,
    LockedOut,
    NotAllowed
}

public sealed record CredentialValidationResult(CredentialValidationStatus Status, Guid UserId, IReadOnlyList<string> Roles);
