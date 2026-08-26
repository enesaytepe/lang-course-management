using LanguageCourseManagement.Application.DTOs.Dashboard;
using LanguageCourseManagement.Application.Services.DashboardService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers.Api;

/// <summary>
/// Dashboard verileri için API endpoint'leri.
/// </summary>
[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
public sealed class DashboardApiController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardApiController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Dashboard istatistiklerini tek bir aggregate sorgu ile getirir.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<DashboardStats>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardStats>> GetStats(CancellationToken cancellationToken)
    {
        var stats = await _dashboardService.GetStatsAsync(cancellationToken);
        return Ok(stats);
    }
}
