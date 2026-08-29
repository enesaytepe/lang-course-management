using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Branches;
using LanguageCourseManagement.Application.Services.BranchService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers.Api;

/// <summary>
/// Şube yönetimi API endpoint'leri.
/// </summary>
[ApiController]
[Route("api/branches")]
[Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
public sealed class BranchApiController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchApiController(IBranchService branchService) => _branchService = branchService;

    /// <summary>
    /// Şubeleri sayfalı olarak listeler.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> veya <c>RegistrationOfficer</c> rolü gerektirir.</remarks>
    [HttpGet]
    [ProducesResponseType<GetListResponse<BranchListResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<GetListResponse<BranchListResponse>>> GetList(
        [FromQuery] PageRequest pageRequest,
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        pageRequest.Normalize();

        return Ok(await _branchService.GetListAsync(pageRequest, search, isActive, cancellationToken));
    }

    /// <summary>
    /// ID'ye göre şube getirir.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> veya <c>RegistrationOfficer</c> rolü gerektirir.</remarks>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<BranchResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<BranchResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _branchService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Yeni şube oluşturur.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<BranchResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BranchResponse>> Create(CreateBranchRequest request, CancellationToken cancellationToken)
    {
        var result = await _branchService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Mevcut şubenin bilgilerini günceller.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<BranchResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BranchResponse>> Update(Guid id, UpdateBranchRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _branchService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Şubeyi soft delete ile siler.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _branchService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
