using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.CourseLevels;
using LanguageCourseManagement.Application.Services.CourseLevelService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers.Api;

/// <summary>
/// Kurs seviyesi yönetimi API endpoint'leri.
/// </summary>
[ApiController]
[Route("api/course-levels")]
[Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
public sealed class CourseLevelApiController : ControllerBase
{
    private readonly ICourseLevelService _courseLevelService;

    public CourseLevelApiController(ICourseLevelService courseLevelService) => _courseLevelService = courseLevelService;

    /// <summary>
    /// Kurs seviyelerini sayfalı olarak listeler.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> veya <c>RegistrationOfficer</c> rolü gerektirir.</remarks>
    [HttpGet]
    [ProducesResponseType<GetListResponse<CourseLevelListResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<GetListResponse<CourseLevelListResponse>>> GetList(
        [FromQuery] PageRequest pageRequest,
        [FromQuery] string? search,
        [FromQuery] Guid? offeredLanguageId,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        pageRequest.Normalize();

        return Ok(await _courseLevelService.GetListAsync(pageRequest, search, offeredLanguageId, isActive ?? true, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// ID'ye göre kurs seviyesi getirir.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> veya <c>RegistrationOfficer</c> rolü gerektirir.</remarks>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<CourseLevelResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CourseLevelResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _courseLevelService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Yeni kurs seviyesi oluşturur.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<CourseLevelResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CourseLevelResponse>> Create(CreateCourseLevelRequest request, CancellationToken cancellationToken)
    {
        var result = await _courseLevelService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Mevcut kurs seviyesinin bilgilerini günceller.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<CourseLevelResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CourseLevelResponse>> Update(Guid id, UpdateCourseLevelRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _courseLevelService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Kurs seviyesini soft delete ile siler.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _courseLevelService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
