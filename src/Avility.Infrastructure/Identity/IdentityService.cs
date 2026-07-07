using Avility.Application.Common.Interfaces;
using Avility.Application.Common.Models;
using Microsoft.AspNetCore.Identity;

namespace Avility.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IDateTime _dateTime;

    public IdentityService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IDateTime dateTime)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dateTime = dateTime;
    }

    public async Task<(bool Succeeded, Guid UserId, IReadOnlyList<string> Errors)> CreateUserAsync(string email, string password, string role)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            CreatedAt = _dateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return (false, Guid.Empty, result.Errors.Select(e => e.Description).ToList());
        }

        await _userManager.AddToRoleAsync(user, role);

        return (true, user.Id, Array.Empty<string>());
    }

    public async Task<CredentialValidationResult> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return new CredentialValidationResult(CredentialValidationStatus.InvalidCredentials, Guid.Empty, Array.Empty<string>());
        }

        if (!user.IsActive)
        {
            return new CredentialValidationResult(CredentialValidationStatus.NotAllowed, Guid.Empty, Array.Empty<string>());
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            return new CredentialValidationResult(CredentialValidationStatus.LockedOut, Guid.Empty, Array.Empty<string>());
        }

        if (!signInResult.Succeeded)
        {
            return new CredentialValidationResult(CredentialValidationStatus.InvalidCredentials, Guid.Empty, Array.Empty<string>());
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new CredentialValidationResult(CredentialValidationStatus.Success, user.Id, roles.ToList());
    }

    public async Task UpdateLastLoginAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return;
        }

        user.LastLoginAt = _dateTime.UtcNow;
        await _userManager.UpdateAsync(user);
    }
    
    public async Task<(string Email, IReadOnlyList<string> Roles)?> GetUserInfoAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user?.Email is null)
        {
            return null;
        }
 
        var roles = await _userManager.GetRolesAsync(user);
        return (user.Email, roles.ToList());
    }
    
    public async Task<bool> SetUserActiveStatusAsync(Guid userId, bool isActive)
    {
         var user = await _userManager.FindByIdAsync(userId.ToString());
         if (user is null)
         {
             return false;
         }
     
         user.IsActive = isActive;
         await _userManager.UpdateAsync(user);
         return true;
    }
}
