using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Application.Services.EnrollmentService;
using LanguageCourseManagement.Application.Services.InstallmentService;
using LanguageCourseManagement.MVC.Models.Api;
using LanguageCourseManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LanguageCourseManagement.MVC.Controllers.Api;

/// <summary>
/// Kayıt yönetimi API endpoint'leri.
/// </summary>
[ApiController]
[Route("api/enrollments")]
[Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
public sealed class EnrollmentApiController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly IInstallmentService _installmentService;

    public EnrollmentApiController(IEnrollmentService enrollmentService, IInstallmentService installmentService)
    {
        _enrollmentService = enrollmentService;
        _installmentService = installmentService;
    }

    /// <summary>
    /// Kayıtları sayfalı olarak listeler.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> veya <c>RegistrationOfficer</c> rolü gerektirir.</remarks>
    [HttpGet]
    [ProducesResponseType<GetListResponse<EnrollmentListItemResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<GetListResponse<EnrollmentListItemResponse>>> GetList(
        [FromQuery] PageRequest pageRequest,
        [FromQuery] string? search,
        [FromQuery] Guid? branchId,
        [FromQuery] EnrollmentStatus? status,
        CancellationToken cancellationToken)
    {
        pageRequest.PageIndex = Math.Max(pageRequest.PageIndex, 0);
        if (pageRequest.PageSize is < 1 or > 100)
            pageRequest.PageSize = 20;

        return Ok(await _enrollmentService.GetListAsync(pageRequest, search, branchId, status, cancellationToken));
    }

    /// <summary>
    /// ID'ye göre kayıt getirir.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> veya <c>RegistrationOfficer</c> rolü gerektirir.</remarks>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<EnrollmentDetailResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<EnrollmentDetailResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _enrollmentService.GetDetailsAsync(id, cancellationToken));
    }

    /// <summary>
    /// Yeni kayıt oluşturur ve tam nakit tahsilat alır.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> veya <c>RegistrationOfficer</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<EnrollmentDetailResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<EnrollmentDetailResponse>> Create(EnrollmentCreateApiModel model, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var result = await _enrollmentService.RegisterAndSettleAsync(model.ToRequest(), userId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Kayıt durumunu günceller.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<EnrollmentDetailResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<EnrollmentDetailResponse>> Update(Guid id, UpdateEnrollmentRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _enrollmentService.UpdateStatusAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Kaydı iptal eder.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<EnrollmentDetailResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<EnrollmentDetailResponse>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _enrollmentService.CancelAsync(id, cancellationToken));
    }

    /// <summary>
    /// Kaydın taksitlerini getirir.
    /// </summary>
    [HttpGet("{id:guid}/installments")]
    [ProducesResponseType(typeof(IReadOnlyList<InstallmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InstallmentResponse>>> GetInstallments(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _installmentService.GetByEnrollmentIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Kayıt için taksit planı oluşturur.
    /// </summary>
    [HttpPost("{id:guid}/installments")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(typeof(IReadOnlyList<InstallmentResponse>), StatusCodes.Status201Created)]
    public async Task<ActionResult<IReadOnlyList<InstallmentResponse>>> CreateInstallmentPlan(Guid id, [FromQuery] int installmentCount, CancellationToken cancellationToken)
    {
        var result = await _installmentService.CreateInstallmentPlanAsync(id, installmentCount, cancellationToken);
        return CreatedAtAction(nameof(GetInstallments), new { id }, result);
    }
}
