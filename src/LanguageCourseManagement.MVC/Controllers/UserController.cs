using LanguageCourseManagement.Application.DTOs.Users;
using LanguageCourseManagement.Application.Services.UserService;
using LanguageCourseManagement.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LanguageCourseManagement.MVC.Controllers;

/// <summary>
/// Kullanıcı yönetimi sayfa endpoint'leri.
/// </summary>
public sealed class UserController : Controller
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Kullanıcı listesini görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Yeni kullanıcı oluşturma ekranını görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public IActionResult Create()
    {
        var model = new UserFormViewModel();
        PopulateRoleList(model);
        return View(model);
    }

    /// <summary>
    /// Kullanıcı düzenleme ekranını görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByIdAsync(id, cancellationToken);
        if (user is null)
            return NotFound();

        var model = new UserFormViewModel
        {
            Id = user.Id,
            UserName = user.UserName,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Role = user.Roles.FirstOrDefault() ?? string.Empty,
            Password = "" // Düzenleme formunda şifre gösterilmez
        };

        PopulateRoleList(model);
        return View(model);
    }

    /// <summary>
    /// Şifre değiştirme ekranını görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public IActionResult ChangePassword(string id)
    {
        if (string.IsNullOrEmpty(id))
            return NotFound();

        var model = new UserChangePasswordViewModel { UserId = id };
        return View(model);
    }

    private static void PopulateRoleList(UserFormViewModel model)
    {
        model.AvailableRoles =
        [
            new SelectListItem("SystemAdmin", "SystemAdmin", model.Role == "SystemAdmin"),
            new SelectListItem("RegistrationOfficer", "RegistrationOfficer", model.Role == "RegistrationOfficer")
        ];
    }
}
