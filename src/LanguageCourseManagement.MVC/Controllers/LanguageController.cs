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

    [HttpPost, Authorize(Roles = "SystemAdmin"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LanguageFormViewModel model, CancellationToken cancellationToken)
    {
        AddWhitespaceValidationError(model);
        if (!ModelState.IsValid) return View(model);

        try
        {
            var language = await _languageService.CreateAsync(ToCreateRequest(model), cancellationToken);
            return RedirectToAction(nameof(Details), new { id = language.Id });
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    [HttpGet, Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        try { return View(ToFormModel(await _languageService.GetByIdAsync(id, cancellationToken))); }
        catch (NotFoundException) { return NotFound(); }
    }

    [HttpPost, Authorize(Roles = "SystemAdmin"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, LanguageFormViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;
        AddWhitespaceValidationError(model);
        if (!ModelState.IsValid) return View(model);
        try
        {
            var language = await _languageService.UpdateAsync(id, ToUpdateRequest(model), cancellationToken);
            return RedirectToAction(nameof(Details), new { id = language.Id });
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
        catch (NotFoundException) { return NotFound(); }
    }

    [HttpGet, Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        try { return View(await _languageService.GetByIdAsync(id, cancellationToken)); }
        catch (NotFoundException) { return NotFound(); }
    }

    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _languageService.DeleteAsync(id, cancellationToken);
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

    private void AddWhitespaceValidationError(LanguageFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name)) ModelState.AddModelError(nameof(model.Name), "Dil adı zorunludur.");
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

    private static CreateOfferedLanguageRequest ToCreateRequest(LanguageFormViewModel model)
    {
        return new()
        {
            Name = model.Name,
            Code = model.Code
        };
    }

    private static UpdateOfferedLanguageRequest ToUpdateRequest(LanguageFormViewModel model)
    {
        return new()
        {
            Name = model.Name,
            Code = model.Code,
            IsActive = model.IsActive
        };
    }
}
