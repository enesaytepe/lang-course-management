using LanguageCourseManagement.Application.DTOs.Dashboard;
using LanguageCourseManagement.Application.Services.DashboardService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers;

/// <summary>
/// Yönetim paneli endpoint'leri.
/// </summary>
[Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
public sealed class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Yönetim panelini görüntüler.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var statistics = await _dashboardService.GetStatisticsAsync(cancellationToken);

        return View(statistics);
    }
}
