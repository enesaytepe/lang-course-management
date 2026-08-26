using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Teachers;
using LanguageCourseManagement.Application.Services.TeacherService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers.Api;

/// <summary>
/// Öğretmen yönetimi API endpoint'leri.
/// </summary>
[ApiController]
[Route("api/teachers")]
[Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
public sealed class TeachersApiController : ControllerBase
{
    private readonly ITeacherService _teacherService;

    public TeachersApiController(ITeacherService teacherService) => _teacherService = teacherService;

    /// <summary>
    /// Öğretmenleri sayfalı olarak listeler.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> veya <c>RegistrationOfficer</c> rolü gerektirir.</remarks>
    [HttpGet]
    [ProducesResponseType<GetListResponse<TeacherListResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<GetListResponse<TeacherListResponse>>> GetList(
        [FromQuery] PageRequest pageRequest,
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        pageRequest.PageIndex = Math.Max(pageRequest.PageIndex, 0);
        if (pageRequest.PageSize is < 1 or > 100)
            pageRequest.PageSize = 20;

        return Ok(await _teacherService.GetListAsync(pageRequest, search, isActive, cancellationToken));
    }

    /// <summary>
    /// ID'ye göre öğretmen getirir.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> veya <c>RegistrationOfficer</c> rolü gerektirir.</remarks>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<TeacherResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TeacherResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _teacherService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Yeni öğretmen oluşturur.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<TeacherResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<TeacherResponse>> Create(CreateTeacherRequest request, CancellationToken cancellationToken)
    {
        var result = await _teacherService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Mevcut öğretmenin bilgilerini günceller.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<TeacherResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TeacherResponse>> Update(Guid id, UpdateTeacherRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _teacherService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Öğretmeni soft delete ile siler.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<TeacherResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TeacherResponse>> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _teacherService.DeleteAsync(id, cancellationToken));
    }

    /// <summary>
    /// Öğretmene müsaitlik dilimi ekler.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpPost("{teacherId:guid}/availabilities")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<TeacherAvailabilityResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<TeacherAvailabilityResponse>> AddAvailability(
        Guid teacherId, CreateTeacherAvailabilityRequest request, CancellationToken cancellationToken)
    {
        var result = await _teacherService.AddAvailabilityAsync(teacherId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = teacherId }, result);
    }

    /// <summary>
    /// Öğretmenin müsaitlik dilimini günceller.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpPut("{teacherId:guid}/availabilities/{availabilityId:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<TeacherAvailabilityResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TeacherAvailabilityResponse>> UpdateAvailability(
        Guid teacherId, Guid availabilityId, UpdateTeacherAvailabilityRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _teacherService.UpdateAvailabilityAsync(teacherId, availabilityId, request, cancellationToken));
    }

    /// <summary>
    /// Öğretmenin müsaitlik dilimini siler.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpDelete("{teacherId:guid}/availabilities/{availabilityId:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<TeacherAvailabilityResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TeacherAvailabilityResponse>> DeleteAvailability(
        Guid teacherId, Guid availabilityId, CancellationToken cancellationToken)
    {
        return Ok(await _teacherService.DeleteAvailabilityAsync(teacherId, availabilityId, cancellationToken));
    }
}
