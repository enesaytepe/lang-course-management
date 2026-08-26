using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.OfferedLanguages;
using LanguageCourseManagement.Application.Services.OfferedLanguageService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers.Api;

/// <summary>
/// Dil yönetimi API endpoint'leri.
/// </summary>
[ApiController]
[Route("api/languages")]
[Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
public sealed class LanguageApiController : ControllerBase
{
    private readonly IOfferedLanguageService _languageService;

    public LanguageApiController(IOfferedLanguageService languageService) => _languageService = languageService;

    /// <summary>
    /// Dilleri sayfalı olarak listeler.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> veya <c>RegistrationOfficer</c> rolü gerektirir.</remarks>
    [HttpGet]
    [ProducesResponseType<GetListResponse<OfferedLanguageListResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<GetListResponse<OfferedLanguageListResponse>>> GetList(
        [FromQuery] PageRequest pageRequest,
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        pageRequest.PageIndex = Math.Max(pageRequest.PageIndex, 0);
        if (pageRequest.PageSize is < 1 or > 100)
            pageRequest.PageSize = 20;

        return Ok(await _languageService.GetListAsync(pageRequest, search, isActive, cancellationToken));
    }

    /// <summary>
    /// ID'ye göre dil getirir.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> veya <c>RegistrationOfficer</c> rolü gerektirir.</remarks>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<OfferedLanguageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<OfferedLanguageResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _languageService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Yeni dil oluşturur.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<OfferedLanguageResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<OfferedLanguageResponse>> Create(CreateOfferedLanguageRequest request, CancellationToken cancellationToken)
    {
        var result = await _languageService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Mevcut dilin bilgilerini günceller.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<OfferedLanguageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<OfferedLanguageResponse>> Update(Guid id, UpdateOfferedLanguageRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _languageService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Dili soft delete ile siler.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<OfferedLanguageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<OfferedLanguageResponse>> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _languageService.DeleteAsync(id, cancellationToken));
    }
}
