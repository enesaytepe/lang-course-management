using AutoMapper;
using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.DTOs.CourseLevels;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.CourseLevelService;
using LanguageCourseManagement.Application.Services.OfferedLanguageService;
using LanguageCourseManagement.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers;

public sealed class CourseLevelController : Controller
{
    private readonly ICourseLevelService _courseLevelService;
    private readonly IOfferedLanguageService _languageService;
    private readonly IMapper _mapper;

    public CourseLevelController(ICourseLevelService courseLevelService, IOfferedLanguageService languageService, IMapper mapper)
    {
        _courseLevelService = courseLevelService;
        _languageService = languageService;
        _mapper = mapper;
    }

    [HttpGet]
    [Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = new CourseLevelFormViewModel();
        await PopulateLanguagesAsync(model, false, cancellationToken);
        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new CourseLevelFormViewModel();
        await PopulateLanguagesAsync(model, false, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseLevelFormViewModel model, CancellationToken cancellationToken)
    {
        AddWhitespaceValidationError(model);
        if (!ModelState.IsValid)
        {
            await PopulateLanguagesAsync(model, false, cancellationToken);
            return View(model);
        }
        try
        {
            var level = await _courseLevelService.CreateAsync(_mapper.Map<CreateCourseLevelRequest>(model), cancellationToken);
            return RedirectToAction(nameof(Details), new { id = level.Id });
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await PopulateLanguagesAsync(model, false, cancellationToken);
            return View(model);
        }
    }

    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var level = await _courseLevelService.GetByIdAsync(id, cancellationToken);
            var model = _mapper.Map<CourseLevelFormViewModel>(level);
            await PopulateLanguagesAsync(model, true, cancellationToken);
            return View(model);
        }
        catch (NotFoundException) { return NotFound(); }
    }

    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CourseLevelFormViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;
        AddWhitespaceValidationError(model);
        if (!ModelState.IsValid)
        {
            await PopulateLanguagesAsync(model, true, cancellationToken);
            return View(model);
        }
        try
        {
            var level = await _courseLevelService.UpdateAsync(id, _mapper.Map<UpdateCourseLevelRequest>(model), cancellationToken);
            return RedirectToAction(nameof(Details), new { id = level.Id });
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await PopulateLanguagesAsync(model, true, cancellationToken);
            return View(model);
        }
        catch (NotFoundException) { return NotFound(); }
    }

    [HttpGet]
    [Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return View(await _courseLevelService.GetByIdAsync(id, cancellationToken));
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
            await _courseLevelService.DeleteAsync(id, cancellationToken);
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

    private async Task PopulateLanguagesAsync(CourseLevelFormViewModel model, bool includeInactive, CancellationToken cancellationToken)
    {
        var pageIndex = 0;
        var options = new List<CourseLevelLanguageOptionViewModel>();
        do
        {
            var languages = await _languageService.GetListAsync(new PageRequest { PageIndex = pageIndex, PageSize = 100 }, null, includeInactive ? null : true, cancellationToken);
            options.AddRange(languages.Items.Select(language => new CourseLevelLanguageOptionViewModel
            {
                Id = language.Id,
                Name = language.Name,
                IsActive = language.IsActive
            }));
            if (!languages.HasNext) break;
            pageIndex++;
        } while (true);
        model.Languages = options.OrderBy(language => language.Name).ToList();
    }

    private void AddWhitespaceValidationError(CourseLevelFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError(nameof(model.Name), "Seviye adı zorunludur.");
    }

}
