using LanguageCourseManagement.Application.DTOs.Students;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.StudentService;
using LanguageCourseManagement.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers;

public sealed class StudentController : Controller
{
    private readonly IStudentService _studentService;

    public StudentController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet]
    [Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
    public IActionResult Index()
    {
        return View(new StudentFormViewModel());
    }

    [HttpGet]
    [Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
    public IActionResult Create()
    {
        return View(new StudentFormViewModel());
    }

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
            var student = await _studentService.CreateAsync(ToCreateRequest(model), cancellationToken);
            return RedirectToAction(nameof(Details), new { id = student.Id });
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var student = await _studentService.GetByIdAsync(id, cancellationToken);
            return View(ToFormModel(student));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

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
            var student = await _studentService.UpdateAsync(id, ToUpdateRequest(model), cancellationToken);
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

    [HttpGet]
    [Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var student = await _studentService.GetByIdAsync(id, cancellationToken);
            return View(ToDetailsViewModel(student));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

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

    private static StudentFormViewModel ToFormModel(StudentResponse student)
    {
        return new()
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            HomePhone = student.HomePhone,
            MobilePhone = student.MobilePhone,
            Email = student.Email,
            IsActive = student.IsActive
        };
    }

    private static StudentDetailsViewModel ToDetailsViewModel(StudentResponse student)
    {
        return new()
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            HomePhone = student.HomePhone,
            MobilePhone = student.MobilePhone,
            Email = student.Email,
            RegistrationDate = student.RegistrationDate,
            IsActive = student.IsActive
        };
    }

    private static CreateStudentRequest ToCreateRequest(StudentFormViewModel model)
    {
        return new()
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            HomePhone = model.HomePhone,
            MobilePhone = model.MobilePhone,
            Email = model.Email
        };
    }

    private static UpdateStudentRequest ToUpdateRequest(StudentFormViewModel model)
    {
        return new()
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            HomePhone = model.HomePhone,
            MobilePhone = model.MobilePhone,
            Email = model.Email,
            IsActive = model.IsActive
        };
    }
}
