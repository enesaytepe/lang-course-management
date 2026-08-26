using AutoMapper;
using LanguageCourseManagement.Application.DTOs.OfferedLanguages;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.OfferedLanguageService;
using LanguageCourseManagement.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers;

/// <summary>
/// Dil yönetimi endpoint'leri.
/// </summary>
public sealed class LanguageController : Controller
{
    private readonly IOfferedLanguageService _languageService;
    private readonly IMapper _mapper;

    public LanguageController(IOfferedLanguageService languageService, IMapper mapper)
    {
        _languageService = languageService;
        _mapper = mapper;
    }

    /// <summary>
    /// Dil listesini görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
    public IActionResult Index()
    {
        return View(new LanguageFormViewModel());
    }

    /// <summary>
    /// Yeni dil oluşturma ekranını görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public IActionResult Create()
    {
        return View(new LanguageFormViewModel());
    }

    /// <summary>
    /// Yeni dil oluşturur.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LanguageFormViewModel model, CancellationToken cancellationToken)
    {
        AddWhitespaceValidationError(model);
        if (!ModelState.IsValid) return View(model);

        try
        {
            var language = await _languageService.CreateAsync(_mapper.Map<CreateOfferedLanguageRequest>(model), cancellationToken);
            return RedirectToAction(nameof(Details), new { id = language.Id });
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    /// <summary>
    /// Dil düzenleme ekranını görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return View(_mapper.Map<LanguageFormViewModel>(await _languageService.GetByIdAsync(id, cancellationToken)));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Dil bilgilerini günceller.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, LanguageFormViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;
        AddWhitespaceValidationError(model);
        if (!ModelState.IsValid) return View(model);
        try
        {
            var language = await _languageService.UpdateAsync(id, _mapper.Map<UpdateOfferedLanguageRequest>(model), cancellationToken);
            return RedirectToAction(nameof(Details), new { id = language.Id });
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Dil detaylarını görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return View(await _languageService.GetByIdAsync(id, cancellationToken));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Dili siler.
    /// </summary>
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
}
