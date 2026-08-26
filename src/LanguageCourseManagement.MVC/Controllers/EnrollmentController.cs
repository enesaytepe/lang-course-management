using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.EnrollmentService;
using LanguageCourseManagement.MVC.Models.ViewModels;
using AutoMapper;
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
    private readonly IMapper _mapper;

    public EnrollmentController(IEnrollmentService enrollmentService, IMapper mapper)
    {
        _enrollmentService = enrollmentService;
        _mapper = mapper;
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
            return View(_mapper.Map<EnrollmentDetailViewModel>(detail));
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
            return View(_mapper.Map<EnrollmentDetailViewModel>(detail));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
