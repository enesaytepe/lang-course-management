using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Facilities;
using LanguageCourseManagement.Application.Services.FacilityService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers.Api;

/// <summary>
/// Tesis yönetimi API endpoint'leri.
/// </summary>
[ApiController]
[Route("api/facilities")]
[Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
public sealed class FacilityApiController : ControllerBase
{
    private readonly IFacilityService _facilityService;

    public FacilityApiController(IFacilityService facilityService)
    {
        _facilityService = facilityService;
    }

    /// <summary>
    /// Aktif tesisleri listeler.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> veya <c>RegistrationOfficer</c> rolü gerektirir.</remarks>
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<FacilityResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<FacilityResponse>>> GetActive(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        return Ok(includeInactive
            ? await _facilityService.GetAllAsync(cancellationToken)
            : await _facilityService.GetActiveAsync(cancellationToken));
    }

    /// <summary>
    /// Tesisleri sayfalı olarak listeler.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> veya <c>RegistrationOfficer</c> rolü gerektirir.</remarks>
    [HttpGet("crud-list")]
    [ProducesResponseType<GetListResponse<FacilityListResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<GetListResponse<FacilityListResponse>>> GetList(
        [FromQuery] PageRequest pageRequest,
        [FromQuery] string? search,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        pageRequest.Normalize();

        return Ok(await _facilityService.GetListAsync(
            pageRequest,
            search,
            includeInactive ? null : true,
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// ID'ye göre tesis getirir.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> veya <c>RegistrationOfficer</c> rolü gerektirir.</remarks>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<FacilityResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<FacilityResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _facilityService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Yeni tesis oluşturur.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<FacilityResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FacilityResponse>> Create(CreateFacilityRequest request, CancellationToken cancellationToken)
    {
        var result = await _facilityService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Mevcut tesisin bilgilerini günceller.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<FacilityResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FacilityResponse>> Update(Guid id, UpdateFacilityRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _facilityService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Tesisi soft delete ile siler.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _facilityService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
