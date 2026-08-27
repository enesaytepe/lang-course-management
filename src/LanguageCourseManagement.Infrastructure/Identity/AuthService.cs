using LanguageCourseManagement.Application.Services.AuthService;
using Microsoft.AspNetCore.Identity;

namespace LanguageCourseManagement.Infrastructure.Identity;

/// <summary>
/// Kimlik doğrulama işlemlerini uygular.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthService(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<AuthResult> PasswordSignInAsync(string userName, string password, bool rememberMe, bool lockoutOnFailure, CancellationToken ct = default)
    {
        var result = await _signInManager.PasswordSignInAsync(userName, password, rememberMe, lockoutOnFailure);
        return new AuthResult(result.Succeeded, result.IsLockedOut);
    }

    public async Task<AuthenticatedUserInfo?> GetUserInfoAsync(string userName, CancellationToken ct = default)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user is null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new AuthenticatedUserInfo(user.UserName ?? userName, roles.ToList().AsReadOnly());
    }

    public async Task SignOutAsync(CancellationToken ct = default)
    {
        await _signInManager.SignOutAsync();
    }
}
