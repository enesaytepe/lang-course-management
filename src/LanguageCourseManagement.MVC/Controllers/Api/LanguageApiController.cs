using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.OfferedLanguages;
using LanguageCourseManagement.Application.Services.OfferedLanguageService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers.Api;

[ApiController]
[Route("api/languages")]
[Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
public sealed class LanguageApiController : ControllerBase
{
    private readonly IOfferedLanguageService _languageService;

    public LanguageApiController(IOfferedLanguageService languageService) => _languageService = languageService;

    [HttpGet]
    [ProducesResponseType<GetListResponse<OfferedLanguageListResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<GetListResponse<OfferedLanguageListResponse>>> GetList(
        [FromQuery] PageRequest pageRequest, [FromQuery] string? search, [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        pageRequest.PageIndex = Math.Max(pageRequest.PageIndex, 0);
        if (pageRequest.PageSize is < 1 or > 100) pageRequest.PageSize = 20;
        return Ok(await _languageService.GetListAsync(pageRequest, search, isActive, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<OfferedLanguageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<OfferedLanguageResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _languageService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<OfferedLanguageResponse>> Create(CreateOfferedLanguageRequest request, CancellationToken cancellationToken)
    {
        var result = await _languageService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<OfferedLanguageResponse>> Update(Guid id, UpdateOfferedLanguageRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _languageService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<OfferedLanguageResponse>> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _languageService.DeleteAsync(id, cancellationToken));
    }
}
