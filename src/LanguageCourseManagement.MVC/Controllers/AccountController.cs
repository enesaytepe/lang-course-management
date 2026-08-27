using LanguageCourseManagement.Application.Services.AuthService;
using LanguageCourseManagement.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers;

/// <summary>
/// Kullanıcı oturum işlemlerini yöneten MVC endpoint'leri.
/// </summary>
public sealed class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Oturum açma ekranını görüntüler.
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = IsLocalUrl(returnUrl) ? returnUrl : null });
    }

    /// <summary>
    /// Kullanıcı adı ve şifre ile oturum açar.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _authService.PasswordSignInAsync(
            model.UserName, model.Password, model.RememberMe, lockoutOnFailure: true, cancellationToken);
        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Hesap çok fazla başarısız deneme nedeniyle kilitlendi. Lütfen daha sonra tekrar deneyin.");
            return View(model);
        }
        if (result.Succeeded)
            return Redirect(IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl! : "/");

        ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı.");
        return View(model);
    }

    /// <summary>
    /// Mevcut kullanıcının oturumunu kapatır.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await _authService.SignOutAsync(cancellationToken);
        return RedirectToAction(nameof(Login));
    }

    /// <summary>
    /// Yetkisiz erişim ekranını görüntüler.
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private bool IsLocalUrl(string? returnUrl)
    {
        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl);
    }
}
