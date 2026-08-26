using LanguageCourseManagement.Application.DTOs.OfferedLanguages;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.OfferedLanguageService;
using LanguageCourseManagement.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers;

public sealed class LanguageController : Controller
{
    private readonly IOfferedLanguageService _languageService;

    public LanguageController(IOfferedLanguageService languageService)
    {
        _languageService = languageService;
    }

    [HttpGet, Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
    public IActionResult Index()
    {
        return View(new LanguageFormViewModel());
    }

    [HttpGet, Authorize(Roles = "SystemAdmin")]
    public IActionResult Create()
    {
        return View(new LanguageFormViewModel());
    }

    [HttpGet, Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        try { return View(ToFormModel(await _languageService.GetByIdAsync(id, cancellationToken))); }
        catch (NotFoundException) { return NotFound(); }
    }

    [HttpGet, Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        try { return View(await _languageService.GetByIdAsync(id, cancellationToken)); }
        catch (NotFoundException) { return NotFound(); }
    }

    private static LanguageFormViewModel ToFormModel(OfferedLanguageResponse language)
    {
        return new()
        {
            Id = language.Id,
            Name = language.Name,
            Code = language.Code,
            IsActive = language.IsActive
        };
    }
}
