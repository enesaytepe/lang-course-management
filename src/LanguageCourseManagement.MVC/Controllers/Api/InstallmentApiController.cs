using LanguageCourseManagement.Application.Services.InstallmentService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers.Api;

/// <summary>
/// Taksit yönetimi API endpoint'leri.
/// </summary>
[ApiController]
[Route("api/installments")]
public sealed class InstallmentApiController : ControllerBase
{
    private readonly IInstallmentService _installmentService;

    public InstallmentApiController(IInstallmentService installmentService) => _installmentService = installmentService;

    /// <summary>
    /// Vadesi geçmiş bekleyen taksitleri Overdue durumuna geçirir.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü gerektirir.</remarks>
    [HttpPost("mark-overdue")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> MarkOverdue(CancellationToken ct)
    {
        await _installmentService.MarkOverdueInstallmentsAsync(ct);
        return Ok(new { message = "Gecikmiş taksitler güncellendi" });
    }

    /// <summary>
    /// Overdue durumundaki taksit sayısını döndürür.
    /// </summary>
    [HttpGet("overdue-count")]
    [Authorize]
    public async Task<IActionResult> GetOverdueCount(CancellationToken ct)
    {
        var count = await _installmentService.GetOverdueCountAsync(ct);
        return Ok(new { count });
    }
}
