using LanguageCourseManagement.Application.Services.AuthService;
using LanguageCourseManagement.MVC.Models.Api;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers.Api;

/// <summary>
/// Kimlik doğrulama API endpoint'leri.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthApiController : ControllerBase
{
    private readonly IAntiforgery _antiforgery;
    private readonly IAuthService _authService;

    public AuthApiController(IAntiforgery antiforgery, IAuthService authService)
    {
        _antiforgery = antiforgery;
        _authService = authService;
    }

    /// <summary>
    /// State-changing API isteklerinde kullanılacak antiforgery token'ı üretir.
    /// </summary>
    /// <remarks>İstek token'ı response body ve <c>X-XSRF-TOKEN</c> header'ı içinde döndürülür.</remarks>
    [AllowAnonymous]
    [HttpGet("csrf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetCsrfToken()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        HttpContext.Response.Headers["X-XSRF-TOKEN"] = tokens.RequestToken;
        return Ok(new { requestToken = tokens.RequestToken });
    }

    /// <summary>
    /// Kullanıcı adı ve şifre ile oturum açar.
    /// </summary>
    /// <remarks>Başarılı oturum açma sonrasında Identity cookie oluşturulur.</remarks>
    [AllowAnonymous]
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<AuthUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    public async Task<ActionResult<AuthUserResponse>> Login(ApiLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.PasswordSignInAsync(
            request.UserName, request.Password, request.RememberMe, lockoutOnFailure: true, cancellationToken);

        if (result.IsLockedOut)
        {
            var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Title = "Account locked",
                Detail = "Hesap çok fazla başarısız deneme nedeniyle kilitlendi. Lütfen daha sonra tekrar deneyin.",
                Status = StatusCodes.Status423Locked,
                Type = "https://api.languagemanagement.edu.tr/problems/account-locked"
            };
            return StatusCode(StatusCodes.Status423Locked, problemDetails);
        }

        if (!result.Succeeded)
        {
            var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Title = "Authentication failed",
                Detail = "Kullanıcı adı veya şifre hatalı.",
                Status = StatusCodes.Status401Unauthorized,
                Type = "https://api.languagemanagement.edu.tr/problems/authentication"
            };
            return Unauthorized(problemDetails);
        }

        var userInfo = await _authService.GetUserInfoAsync(request.UserName, cancellationToken);
        if (userInfo is null)
        {
            var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Title = "Authentication failed",
                Detail = "Kimlik doğrulama başarısız.",
                Status = StatusCodes.Status401Unauthorized,
                Type = "https://api.languagemanagement.edu.tr/problems/authentication"
            };
            return Unauthorized(problemDetails);
        }

        return Ok(new AuthUserResponse { UserName = userInfo.UserName, Roles = userInfo.Roles.ToArray() });
    }

    /// <summary>
    /// Mevcut kullanıcının oturumunu kapatır.
    /// </summary>
    /// <remarks>Kimlik doğrulama ve antiforgery doğrulaması gerektirir.</remarks>
    [Authorize]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await _authService.SignOutAsync(cancellationToken);
        return NoContent();
    }
}
