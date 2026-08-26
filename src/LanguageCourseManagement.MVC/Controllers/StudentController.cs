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

}
