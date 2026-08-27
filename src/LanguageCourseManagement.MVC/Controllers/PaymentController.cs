using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.PaymentService;
using LanguageCourseManagement.Domain.Enums;
using LanguageCourseManagement.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers;

/// <summary>
/// Tahsilat yönetimi endpoint'leri.
/// </summary>
[Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
public sealed class PaymentController : Controller
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// Tahsilat listesini görüntüler.
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Yeni tahsilat oluşturma ekranını görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
    public async Task<IActionResult> Create(Guid? enrollmentId, CancellationToken cancellationToken = default)
    {
        var model = new PaymentFormViewModel();

        if (enrollmentId.HasValue)
            model.EnrollmentId = enrollmentId.Value;

        var enrollments = await _paymentService.GetUnsettledEnrollmentsAsync(cancellationToken);

        model.UnsettledEnrollments = enrollments
            .Select(e => new EnrollmentOptionViewModel
            {
                Id = e.Id,
                StudentName = e.StudentName,
                CourseName = e.CourseName,
                BranchName = e.BranchName,
                FinalAmount = e.FinalAmount,
                PaymentType = e.PaymentType
            })
            .ToList();

        var installmentEnrollmentIds = enrollments
            .Where(e => e.PaymentType == PaymentType.Installment.ToString())
            .Select(e => e.Id)
            .ToList();

        if (installmentEnrollmentIds.Count > 0)
        {
            var pendingInstallments = await _paymentService.GetPendingInstallmentsByEnrollmentIdsAsync(installmentEnrollmentIds, cancellationToken);

            model.EnrollmentInstallments = pendingInstallments
                .GroupBy(i => i.EnrollmentId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(i => new InstallmentOptionViewModel
                    {
                        Id = i.Id,
                        InstallmentNumber = i.InstallmentNumber,
                        Amount = i.Amount,
                        DueDate = i.DueDate,
                        Status = i.Status
                    }).ToList());
        }

        foreach (var id in installmentEnrollmentIds)
        {
            if (!model.EnrollmentInstallments.ContainsKey(id))
                model.EnrollmentInstallments[id] = [];
        }

        if (enrollmentId.HasValue)
        {
            var selected = enrollments.FirstOrDefault(e => e.Id == enrollmentId.Value);
            if (selected is not null)
            {
                model.StudentName = selected.StudentName;
                model.CourseName = selected.CourseName;
                model.BranchName = selected.BranchName;
                model.FinalAmount = selected.FinalAmount;
                model.EnrollmentPaymentType = selected.PaymentType;
            }
        }

        return View(model);
    }

    /// <summary>
    /// Tahsilat detaylarını görüntüler.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _paymentService.GetByIdAsync(id, cancellationToken);
            var model = new PaymentDetailsViewModel
            {
                Id = response.Id,
                EnrollmentId = response.EnrollmentId,
                StudentName = response.StudentName,
                CourseName = response.CourseName,
                BranchName = response.BranchName,
                Amount = response.Amount,
                Method = response.Method,
                Status = response.Status,
                SettledAt = response.SettledAt,
                Description = response.Description
            };
            return View(model);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
