using LanguageCourseManagement.Infrastructure.Identity;
using LanguageCourseManagement.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers;

/// <summary>
/// Kullanıcı oturum işlemlerini yöneten MVC endpoint'leri.
/// </summary>
public sealed class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
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

        var result = await _signInManager.PasswordSignInAsync(
            model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
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
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
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
