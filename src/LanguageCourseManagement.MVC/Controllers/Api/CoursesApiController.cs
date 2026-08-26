using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Courses;
using LanguageCourseManagement.Application.Services.CourseService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers.Api;

/// <summary>
/// Kurs yönetimi API endpoint'leri.
/// </summary>
[ApiController]
[Route("api/courses")]
[Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
public sealed class CoursesApiController : ControllerBase
{
    private readonly ICourseService _service;

    public CoursesApiController(ICourseService service) => _service = service;

    /// <summary>
    /// Kursları sayfalı olarak listeler.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> veya <c>RegistrationOfficer</c> rolü gerektirir.</remarks>
    [HttpGet]
    [ProducesResponseType<GetListResponse<CourseListResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<GetListResponse<CourseListResponse>>> GetList(
        [FromQuery] PageRequest pageRequest,
        [FromQuery] string? search,
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? offeredLanguageId,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken,
        [FromQuery] bool showDeleted = false)
    {
        pageRequest.PageIndex = Math.Max(pageRequest.PageIndex, 0);
        if (pageRequest.PageSize is < 1 or > 100)
            pageRequest.PageSize = 20;

        return Ok(await _service.GetListAsync(pageRequest, search, branchId, offeredLanguageId, isActive, showDeleted, cancellationToken));
    }

    /// <summary>
    /// ID'ye göre kurs getirir.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> veya <c>RegistrationOfficer</c> rolü gerektirir.</remarks>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<CourseResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CourseResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Kurssa ait ders programını getirir.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> veya <c>RegistrationOfficer</c> rolü gerektirir.</remarks>
    [HttpGet("{id:guid}/schedules")]
    [ProducesResponseType<IReadOnlyList<CourseScheduleItemDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CourseScheduleItemDto>>> GetSchedules(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetSchedulesAsync(id, cancellationToken));
    }

    /// <summary>
    /// Yeni kurs oluşturur.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<CourseResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<CourseResponse>> Create(CreateCourseRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Mevcut kursun bilgilerini günceller.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<CourseResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CourseResponse>> Update(Guid id, UpdateCourseRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _service.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Kursu soft delete ile siler.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Kurs için uygun öğretmenleri listeler.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpPost("eligible-teachers")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<IReadOnlyList<EligibleTeacherResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EligibleTeacherResponse>>> EligibleTeachers(GetEligibleTeachersRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetEligibleTeachersAsync(request.BranchId, request.OfferedLanguageId, request.CourseLevelId, request.StartDate, request.EndDate, request.Schedules, request.ExcludeCourseId, cancellationToken));
    }

    /// <summary>
    /// Kurs için uygun derslikleri listeler.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpPost("eligible-classrooms")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<IReadOnlyList<EligibleClassroomResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EligibleClassroomResponse>>> EligibleClassrooms(GetEligibleClassroomsRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetEligibleClassroomsAsync(request.BranchId, request.StartDate, request.EndDate, request.Schedules, request.ExcludeCourseId, cancellationToken));
    }
}
