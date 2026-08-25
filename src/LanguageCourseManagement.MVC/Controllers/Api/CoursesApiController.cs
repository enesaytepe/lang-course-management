using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Courses;
using LanguageCourseManagement.Application.Services.CourseService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers.Api;

[ApiController][Route("api/courses")][Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
public sealed class CoursesApiController : ControllerBase
{
    private readonly ICourseService _service;
    public CoursesApiController(ICourseService service) => _service = service;

    [HttpGet] public async Task<ActionResult<GetListResponse<CourseListResponse>>> GetList([FromQuery] PageRequest pageRequest, [FromQuery] string? search, [FromQuery] Guid? branchId, [FromQuery] Guid? offeredLanguageId, [FromQuery] bool? isActive, CancellationToken cancellationToken)
    { pageRequest.PageIndex = Math.Max(pageRequest.PageIndex, 0); if (pageRequest.PageSize is < 1 or > 100) pageRequest.PageSize = 20; return Ok(await _service.GetListAsync(pageRequest, search, branchId, offeredLanguageId, isActive, cancellationToken)); }
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CourseResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetByIdAsync(id, cancellationToken));
    }

    [HttpGet("{id:guid}/schedules")]
    public async Task<ActionResult<IReadOnlyList<CourseScheduleItemDto>>> GetSchedules(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetSchedulesAsync(id, cancellationToken));
    }

    [HttpPost][Authorize(Roles = "SystemAdmin")][ValidateAntiForgeryToken] public async Task<ActionResult<CourseResponse>> Create(CreateCourseRequest request, CancellationToken cancellationToken) { var result = await _service.CreateAsync(request, cancellationToken); return CreatedAtAction(nameof(GetById), new { id = result.Id }, result); }
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<CourseResponse>> Update(Guid id, UpdateCourseRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _service.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<CourseResponse>> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.DeleteAsync(id, cancellationToken));
    }

    [HttpPost("eligible-teachers")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<IReadOnlyList<EligibleTeacherResponse>>> EligibleTeachers(GetEligibleTeachersRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetEligibleTeachersAsync(request.BranchId, request.OfferedLanguageId, request.CourseLevelId, request.StartDate, request.EndDate, request.Schedules, request.ExcludeCourseId, cancellationToken));
    }

    [HttpPost("eligible-classrooms")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<IReadOnlyList<EligibleClassroomResponse>>> EligibleClassrooms(GetEligibleClassroomsRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetEligibleClassroomsAsync(request.BranchId, request.StartDate, request.EndDate, request.Schedules, request.ExcludeCourseId, cancellationToken));
    }
}
