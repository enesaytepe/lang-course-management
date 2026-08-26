using AutoMapper;
using LanguageCourseManagement.Application.DTOs.Students;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.EnrollmentService;
using LanguageCourseManagement.Application.Services.StudentService;
using LanguageCourseManagement.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers;

/// <summary>
/// Öğrenci yönetimi endpoint'leri.
/// </summary>
public sealed class StudentController : Controller
{
    private readonly IStudentService _studentService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IMapper _mapper;

    public StudentController(IStudentService studentService, IEnrollmentService enrollmentService, IMapper mapper)
    {
        _studentService = studentService;
        _enrollmentService = enrollmentService;
        _mapper = mapper;
    }

    /// <summary>
    /// Öğrenci listesini görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
    public IActionResult Index()
    {
        return View(new StudentFormViewModel());
    }

    /// <summary>
    /// Yeni öğrenci oluşturma ekranını görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
    public IActionResult Create()
    {
        return View(new StudentFormViewModel());
    }

    /// <summary>
    /// Yeni öğrenci oluşturur.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentFormViewModel model, CancellationToken cancellationToken)
    {
        AddWhitespaceValidationErrors(model);

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var student = await _studentService.CreateAsync(_mapper.Map<CreateStudentRequest>(model), cancellationToken);
            return RedirectToAction(nameof(Details), new { id = student.Id });
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    /// <summary>
    /// Öğrenci düzenleme ekranını görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var student = await _studentService.GetByIdAsync(id, cancellationToken);
            return View(_mapper.Map<StudentFormViewModel>(student));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Öğrenci bilgilerini günceller.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid id,
        StudentFormViewModel model,
        CancellationToken cancellationToken)
    {
        AddWhitespaceValidationErrors(model);

        if (!ModelState.IsValid)
        {
            model.Id = id;
            return View(model);
        }

        try
        {
            var student = await _studentService.UpdateAsync(id, _mapper.Map<UpdateStudentRequest>(model), cancellationToken);
            return RedirectToAction(nameof(Details), new { id = student.Id });
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            model.Id = id;
            return View(model);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Öğrenci detaylarını görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var student = await _studentService.GetByIdAsync(id, cancellationToken);
            var enrollments = await _enrollmentService.GetByStudentIdAsync(id, cancellationToken);
            var viewModel = _mapper.Map<StudentDetailsViewModel>(student);
            viewModel.Enrollments = enrollments;
            return View(viewModel);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Öğrenciyi siler.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _studentService.DeleteAsync(id, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (BusinessException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    private void AddWhitespaceValidationErrors(StudentFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.FirstName))
            ModelState.AddModelError(nameof(model.FirstName), "Ad zorunludur.");

        if (string.IsNullOrWhiteSpace(model.LastName))
            ModelState.AddModelError(nameof(model.LastName), "Soyad zorunludur.");

        if (string.IsNullOrWhiteSpace(model.MobilePhone))
            ModelState.AddModelError(nameof(model.MobilePhone), "Cep telefonu zorunludur.");
    }
}
