using Avility.Application.Common.Models;

namespace Avility.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<(bool Succeeded, Guid UserId, IReadOnlyList<string> Errors)> CreateUserAsync(string email, string password, string role);
    Task<CredentialValidationResult> ValidateCredentialsAsync(string email, string password);
    Task UpdateLastLoginAsync(Guid userId);
    Task<(string Email, IReadOnlyList<string> Roles)?> GetUserInfoAsync(Guid userId);
    Task<bool> SetUserActiveStatusAsync(Guid userId, bool isActive);
    Task<(int Active, int Inactive)> GetUserCountsAsync();
    Task<string?> GeneratePasswordResetTokenAsync(string email);
    Task<(bool Succeeded, IReadOnlyList<string> Errors)> ResetPasswordAsync(string email, string token, string newPassword);
}
