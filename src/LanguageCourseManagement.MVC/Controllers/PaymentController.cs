using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.EnrollmentService;
using LanguageCourseManagement.Application.Services.PaymentService;
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
    private readonly IEnrollmentService _enrollmentService;

    public PaymentController(IPaymentService paymentService, IEnrollmentService enrollmentService)
    {
        _paymentService = paymentService;
        _enrollmentService = enrollmentService;
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

        // Henüz tahsilat yapılmamış aktif kayıtları dropdown için getir
        var enrollments = await _enrollmentService.GetListAsync(cancellationToken);
        var unsettled = enrollments
            .Where(e => e.Status == "Active" && !e.IsSettled)
            .Select(e => new EnrollmentOptionViewModel
            {
                Id = e.Id,
                StudentName = e.StudentName,
                CourseName = e.CourseName,
                BranchName = string.Empty,
                FinalAmount = e.FinalAmount
            })
            .ToList();

        model.UnsettledEnrollments = unsettled;

        // Eğer enrollmentId belirtilmişse ilgili kaydın bilgilerini doldur
        if (enrollmentId.HasValue)
        {
            var selected = unsettled.FirstOrDefault(e => e.Id == enrollmentId.Value);
            if (selected is not null)
            {
                model.StudentName = selected.StudentName;
                model.CourseName = selected.CourseName;
                model.BranchName = selected.BranchName;
                model.FinalAmount = selected.FinalAmount;
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
