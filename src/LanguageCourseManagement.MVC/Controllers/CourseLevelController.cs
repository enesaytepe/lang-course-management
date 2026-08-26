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

    public CourseLevelController(ICourseLevelService courseLevelService, IOfferedLanguageService languageService)
    {
        _courseLevelService = courseLevelService;
        _languageService = languageService;
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

    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var level = await _courseLevelService.GetByIdAsync(id, cancellationToken);
            var model = ToFormModel(level);
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

    private static CourseLevelFormViewModel ToFormModel(CourseLevelResponse level)
    {
        return new()
        {
            Id = level.Id,
            OfferedLanguageId = level.OfferedLanguageId,
            Name = level.Name,
            Description = level.Description,
            Order = level.Order,
            IsActive = level.IsActive
        };
    }
}
