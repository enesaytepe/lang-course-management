using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.DTOs.Courses;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.BranchService;
using LanguageCourseManagement.Application.Services.CourseLevelService;
using LanguageCourseManagement.Application.Services.CourseService;
using LanguageCourseManagement.Application.Services.OfferedLanguageService;
using LanguageCourseManagement.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers;

public sealed class CourseController : Controller
{
    private readonly ICourseService _courseService;
    private readonly IBranchService _branchService;
    private readonly IOfferedLanguageService _languageService;
    private readonly ICourseLevelService _levelService;

    public CourseController(ICourseService courseService, IBranchService branchService, IOfferedLanguageService languageService, ICourseLevelService levelService)
    { _courseService = courseService; _branchService = branchService; _languageService = languageService; _levelService = levelService; }

    [HttpGet]
    [Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await CreateFormModelAsync(cancellationToken));
    }

    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        return View(await CreateFormModelAsync(cancellationToken));
    }

    [HttpGet][Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        try { var response = await _courseService.GetByIdAsync(id, cancellationToken); var model = ToFormModel(response, await _courseService.GetSchedulesAsync(id, cancellationToken)); await PopulateOptionsAsync(model, true, cancellationToken); return View(model); }
        catch (NotFoundException) { return NotFound(); }
    }

    [HttpGet][Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        try { var response = await _courseService.GetByIdAsync(id, cancellationToken); return View(CourseDetailsViewModel.FromResponse(response, await _courseService.GetSchedulesAsync(id, cancellationToken))); }
        catch (NotFoundException) { return NotFound(); }
    }

    private async Task<CourseFormViewModel> CreateFormModelAsync(CancellationToken cancellationToken)
    { var model = new CourseFormViewModel(); await PopulateOptionsAsync(model, false, cancellationToken); return model; }

    private async Task PopulateOptionsAsync(CourseFormViewModel model, bool includeInactive, CancellationToken cancellationToken)
    {
        const int size = 100; var branches = await _branchService.GetListAsync(new PageRequest { PageIndex = 0, PageSize = size }, null, includeInactive ? null : true, cancellationToken);
        model.Branches = branches.Items.Select(item => new CourseBranchOptionViewModel { Id = item.Id, Name = item.Name ?? string.Empty, IsActive = item.IsActive }).OrderBy(x => x.Name).ToList();
        var languages = await _languageService.GetListAsync(new PageRequest { PageIndex = 0, PageSize = size }, null, includeInactive ? null : true, cancellationToken);
        model.Languages = languages.Items.Select(item => new CourseLanguageOptionViewModel { Id = item.Id, Name = item.Name, IsActive = item.IsActive }).OrderBy(x => x.Name).ToList();
        var levels = await _levelService.GetListAsync(new PageRequest { PageIndex = 0, PageSize = size }, null, model.OfferedLanguageId, includeInactive ? null : true, cancellationToken);
        model.Levels = levels.Items.Select(item => new CourseLevelOptionViewModel { Id = item.Id, Name = item.Name, IsActive = item.IsActive }).OrderBy(x => x.Name).ToList();
        if (model.BranchId.HasValue && model.OfferedLanguageId.HasValue && model.Schedules.Count > 0)
        {
            model.EligibleTeachers = await _courseService.GetEligibleTeachersAsync(model.BranchId.Value, model.OfferedLanguageId.Value, model.CourseLevelId.GetValueOrDefault(), model.StartDate.GetValueOrDefault(), model.EndDate.GetValueOrDefault(), model.Schedules, model.Id, cancellationToken);
            model.EligibleClassrooms = await _courseService.GetEligibleClassroomsAsync(model.BranchId.Value, model.StartDate.GetValueOrDefault(), model.EndDate.GetValueOrDefault(), model.Schedules, model.Id, cancellationToken);
        }
    }

    private static CourseFormViewModel ToFormModel(CourseResponse response, IReadOnlyList<CourseScheduleItemDto> schedules)
    {
        return new()
        {
            Id = response.Id,
            Name = response.Name,
            BranchId = response.BranchId,
            OfferedLanguageId = response.OfferedLanguageId,
            CourseLevelId = response.CourseLevelId,
            TeacherId = response.TeacherId,
            ClassroomId = response.ClassroomId,
            StartDate = response.StartDate,
            EndDate = response.EndDate,
            Capacity = response.Capacity,
            TuitionFee = response.TuitionFee,
            Status = response.Status,
            IsActive = response.IsActive,
            Schedules = schedules.ToList()
        };
    }

}
