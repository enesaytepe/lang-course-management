using LanguageCourseManagement.Application.Services.DashboardService;
using LanguageCourseManagement.MVC.Models.ViewModels;
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
        var stats = await _dashboardService.GetStatsAsync(cancellationToken);

        return View(new DashboardViewModel
        {
            ActiveBranchCount = stats.ActiveBranchCount,
            ActiveClassroomCount = stats.ActiveClassroomCount,
            ActiveTeacherCount = stats.ActiveTeacherCount,
            ActiveStudentCount = stats.ActiveStudentCount,
            ActiveCourseCount = stats.ActiveCourseCount,
            TotalEnrollmentCount = stats.TotalEnrollmentCount,
            ActiveEnrollments = stats.ActiveEnrollments,
            TotalSettledAmount = stats.TotalSettledAmount,
            MonthlyRevenue = stats.MonthlyRevenue,
            PendingPaymentCount = stats.PendingPaymentCount,
            OverdueInstallmentCount = stats.OverdueInstallmentCount
        });
    }
}
