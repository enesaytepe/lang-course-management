using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.CourseLevels;
using LanguageCourseManagement.Application.Services.CourseLevelService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers.Api;

[ApiController]
[Route("api/course-levels")]
[Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
public sealed class CourseLevelApiController : ControllerBase
{
    private readonly ICourseLevelService _courseLevelService;

    public CourseLevelApiController(ICourseLevelService courseLevelService) => _courseLevelService = courseLevelService;

    [HttpGet]
    [ProducesResponseType<GetListResponse<CourseLevelListResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<GetListResponse<CourseLevelListResponse>>> GetList(
        [FromQuery] PageRequest pageRequest,
        [FromQuery] string? search,
        [FromQuery] Guid? offeredLanguageId,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        pageRequest.PageIndex = Math.Max(pageRequest.PageIndex, 0);
        if (pageRequest.PageSize is < 1 or > 100)
            pageRequest.PageSize = 20;
        return Ok(await _courseLevelService.GetListAsync(pageRequest, search, offeredLanguageId, isActive, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<CourseLevelResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CourseLevelResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _courseLevelService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<CourseLevelResponse>> Create(CreateCourseLevelRequest request, CancellationToken cancellationToken)
    {
        var result = await _courseLevelService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<CourseLevelResponse>> Update(Guid id, UpdateCourseLevelRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _courseLevelService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<CourseLevelResponse>> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _courseLevelService.DeleteAsync(id, cancellationToken));
    }
}
