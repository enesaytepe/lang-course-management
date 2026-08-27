using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.Services.AuditLogService;
using LanguageCourseManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers.Api;

/// <summary>
/// Audit log API endpoint'leri.
/// </summary>
[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = "SystemAdmin")]
public sealed class AuditLogApiController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogApiController(IAuditLogService auditLogService) => _auditLogService = auditLogService;

    /// <summary>
    /// Audit log kayıtlarını sayfalı olarak listeler.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<GetListResponse<AuditLogListResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<GetListResponse<AuditLogListResponse>>> GetList(
        [FromQuery] PageRequest pageRequest,
        [FromQuery] string? entityName,
        [FromQuery] AuditAction? action,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        pageRequest.PageIndex = Math.Max(pageRequest.PageIndex, 0);
        if (pageRequest.PageSize is < 1 or > 100)
            pageRequest.PageSize = 20;

        return Ok(await _auditLogService.GetListAsync(pageRequest, entityName, action, search, cancellationToken));
    }

    /// <summary>
    /// ID'ye göre audit log kaydını getirir.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<AuditLogResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuditLogResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _auditLogService.GetByIdAsync(id, cancellationToken));
    }
}
