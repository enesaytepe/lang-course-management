using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Payments;
using LanguageCourseManagement.Application.Services.PaymentService;
using LanguageCourseManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LanguageCourseManagement.MVC.Controllers.Api;

/// <summary>
/// Tahsilat yönetimi API endpoint'leri.
/// </summary>
[ApiController]
[Route("api/payments")]
[Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
public sealed class PaymentApiController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentApiController(IPaymentService paymentService) => _paymentService = paymentService;

    /// <summary>
    /// Tahsilatları sayfalı olarak listeler.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> veya <c>RegistrationOfficer</c> rolü gerektirir.</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(GetListResponse<PaymentListResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetListResponse<PaymentListResponse>>> GetList(
        [FromQuery] PageRequest pageRequest,
        [FromQuery] string? search,
        [FromQuery] Guid? branchId,
        [FromQuery] PaymentStatus? status,
        CancellationToken cancellationToken)
    {
        pageRequest.PageIndex = Math.Max(pageRequest.PageIndex, 0);
        if (pageRequest.PageSize is < 1 or > 100)
            pageRequest.PageSize = 20;

        return Ok(await _paymentService.GetListAsync(pageRequest, search, branchId, status: status, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// ID'ye göre tahsilat getirir.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> veya <c>RegistrationOfficer</c> rolü gerektirir.</remarks>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<PaymentResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaymentResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _paymentService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Nakit tam tahsilat gerçekleştirir.
    /// </summary>
    /// <remarks><c>SystemAdmin</c> veya <c>RegistrationOfficer</c> rolü ve antiforgery doğrulaması gerektirir.</remarks>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ProducesResponseType<PaymentResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<PaymentResponse>> Create(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var result = await _paymentService.CreateAsync(request, userId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
