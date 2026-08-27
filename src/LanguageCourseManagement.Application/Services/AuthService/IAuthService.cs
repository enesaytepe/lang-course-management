namespace LanguageCourseManagement.Application.Services.AuthService;

/// <summary>
/// Kimlik doğrulama işlemlerini tanımlar.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Kullanıcı adı ve şifre ile oturum açar.
    /// </summary>
    Task<AuthResult> PasswordSignInAsync(string userName, string password, bool rememberMe, bool lockoutOnFailure, CancellationToken ct = default);

    /// <summary>
    /// Kullanıcı adına göre kimlik bilgilerini getirir.
    /// </summary>
    Task<AuthenticatedUserInfo?> GetUserInfoAsync(string userName, CancellationToken ct = default);

    /// <summary>
    /// Mevcut kullanıcının oturumunu kapatır.
    /// </summary>
    Task SignOutAsync(CancellationToken ct = default);
}

/// <summary>
/// Oturum açma işleminin sonucunu temsil eder.
/// </summary>
public sealed record AuthResult(bool Succeeded, bool IsLockedOut);

/// <summary>
/// Kimlik doğrulanmış kullanıcının bilgilerini temsil eder.
/// </summary>
public sealed record AuthenticatedUserInfo(string UserName, IReadOnlyCollection<string> Roles);
