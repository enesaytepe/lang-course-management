using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.EnrollmentService;
using LanguageCourseManagement.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers;

/// <summary>
/// Kayıt yönetimi endpoint'leri
/// </summary>
[Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
public sealed class EnrollmentController : Controller
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    /// <summary>
    /// Kayıt listesini görüntüler.
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        return View(new EnrollmentCreateViewModel());
    }

    /// <summary>
    /// Yeni kayıt oluşturma ekranını görüntüler.
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        return View(new EnrollmentCreateViewModel());
    }

    /// <summary>
    /// Kayıt detaylarını görüntüler.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var detail = await _enrollmentService.GetDetailsAsync(id, cancellationToken);
            return View(ToViewModel(detail));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Kayıt düzenleme ekranını görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var detail = await _enrollmentService.GetDetailsAsync(id, cancellationToken);
            return View(ToViewModel(detail));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    private static EnrollmentDetailViewModel ToViewModel(EnrollmentDetailResponse detail)
    {
        return new()
        {
            Id = detail.Id,
            StudentId = detail.StudentId,
            StudentName = detail.StudentName,
            CourseId = detail.CourseId,
            CourseName = detail.CourseName,
            TuitionFee = detail.TuitionFee,
            DiscountAmount = detail.DiscountAmount,
            FinalAmount = detail.FinalAmount,
            Status = detail.Status,
            IsSettled = detail.IsSettled,
            PaymentId = detail.PaymentId
        };
    }
}
