using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Classrooms;
using LanguageCourseManagement.Application.Services.ClassroomService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers.Api;

/// <summary>
/// Derslik yönetimi API endpoint'leri.
/// </summary>
[ApiController]
[Route("api/classrooms")]
[Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
public sealed class ClassroomsApiController : ControllerBase
{
    private readonly IClassroomService _classroomService;

    public ClassroomsApiController(IClassroomService classroomService)
    {
        _classroomService = classroomService;
    }

    /// <summary>
    /// Derslikleri sayfalı olarak listeler.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> veya <c>RegistrationOfficer</c> rolü gerektirir.</remarks>
    [HttpGet]
    [ProducesResponseType<GetListResponse<ClassroomListResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<GetListResponse<ClassroomListResponse>>> GetList(
        [FromQuery] PageRequest pageRequest,
        [FromQuery] string? search,
        [FromQuery] Guid? branchId,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        pageRequest.PageIndex = Math.Max(pageRequest.PageIndex, 0);
        if (pageRequest.PageSize is < 1 or > 100)
            pageRequest.PageSize = 20;

        return Ok(await _classroomService.GetListAsync(pageRequest, search, branchId, isActive ?? true, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// ID'ye göre derslik getirir.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> veya <c>RegistrationOfficer</c> rolü gerektirir.</remarks>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ClassroomResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ClassroomResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _classroomService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Yeni derslik oluşturur.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<ClassroomResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ClassroomResponse>> Create(CreateClassroomRequest request, CancellationToken cancellationToken)
    {
        var result = await _classroomService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Mevcut dersliğin bilgilerini günceller.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<ClassroomResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ClassroomResponse>> Update(Guid id, UpdateClassroomRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _classroomService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Dersliği soft delete ile siler.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _classroomService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
