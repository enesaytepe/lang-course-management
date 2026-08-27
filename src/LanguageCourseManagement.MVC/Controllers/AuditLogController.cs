using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers;

/// <summary>
/// Audit log yönetimi MVC endpoint'leri.
/// </summary>
public sealed class AuditLogController : Controller
{
    /// <summary>
    /// Audit log listesini görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public IActionResult Index()
    {
        return View();
    }
}
