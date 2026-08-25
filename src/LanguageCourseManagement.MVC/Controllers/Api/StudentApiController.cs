using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Students;
using LanguageCourseManagement.Application.Services.StudentService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers.Api;

[ApiController]
[Route("api/students")]
[Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
public sealed class StudentApiController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentApiController(IStudentService studentService) => _studentService = studentService;

    [HttpGet]
    [ProducesResponseType<GetListResponse<StudentListResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<GetListResponse<StudentListResponse>>> GetList(
        [FromQuery] PageRequest pageRequest,
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        pageRequest.PageIndex = Math.Max(pageRequest.PageIndex, 0);
        if (pageRequest.PageSize is < 1 or > 100)
            pageRequest.PageSize = 20;

        return Ok(await _studentService.GetListAsync(pageRequest, search, isActive, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<StudentResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<StudentResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _studentService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<StudentResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<StudentResponse>> Create(
        CreateStudentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _studentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<StudentResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<StudentResponse>> Update(
        Guid id,
        UpdateStudentRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _studentService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<StudentResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<StudentResponse>> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _studentService.DeleteAsync(id, cancellationToken));
    }
}
