using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Facilities;
using LanguageCourseManagement.Application.Services.FacilityService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers.Api;

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

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<FacilityResponse>>> GetActive(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        return Ok(includeInactive
            ? await _facilityService.GetAllAsync(cancellationToken)
            : await _facilityService.GetActiveAsync(cancellationToken));
    }

    [HttpGet("crud-list")]
    [ProducesResponseType<GetListResponse<FacilityListResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<GetListResponse<FacilityListResponse>>> GetList(
        [FromQuery] PageRequest pageRequest,
        [FromQuery] string? search,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        pageRequest.PageIndex = Math.Max(pageRequest.PageIndex, 0);
        if (pageRequest.PageSize is < 1 or > 100)
            pageRequest.PageSize = 20;

        return Ok(await _facilityService.GetListAsync(
            pageRequest,
            search,
            includeInactive ? null : true,
            cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<FacilityResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<FacilityResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await _facilityService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<FacilityResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<FacilityResponse>> Create(
        CreateFacilityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _facilityService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<FacilityResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<FacilityResponse>> Update(
        Guid id,
        UpdateFacilityRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _facilityService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<FacilityResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<FacilityResponse>> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await _facilityService.DeleteAsync(id, cancellationToken));
    }
}
