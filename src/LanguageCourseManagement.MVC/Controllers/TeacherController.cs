using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.DTOs.Teachers;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.BranchService;
using LanguageCourseManagement.Application.Services.OfferedLanguageService;
using LanguageCourseManagement.Application.Services.TeacherService;
using LanguageCourseManagement.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LanguageCourseManagement.MVC.Controllers;

public sealed class TeacherController : Controller
{
    private readonly ITeacherService _teacherService;
    private readonly IOfferedLanguageService _offeredLanguageService;
    private readonly IBranchService _branchService;

    public TeacherController(
        ITeacherService teacherService,
        IOfferedLanguageService offeredLanguageService,
        IBranchService branchService)
    {
        _teacherService = teacherService;
        _offeredLanguageService = offeredLanguageService;
        _branchService = branchService;
    }

    [HttpGet]
    [Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
    public IActionResult Index()
    {
        return View(new TeacherFormViewModel());
    }

    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new TeacherFormViewModel();
        await PopulateFormSelectListsAsync(model, cancellationToken: cancellationToken);
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TeacherFormViewModel model, CancellationToken cancellationToken)
    {
        AddWhitespaceValidationErrors(model);
        if (!ModelState.IsValid)
        {
            await PopulateFormSelectListsAsync(model, cancellationToken: cancellationToken);
            return View(model);
        }

        try
        {
            var teacher = await _teacherService.CreateAsync(ToCreateRequest(model), cancellationToken);
            return RedirectToAction(nameof(Details), new { id = teacher.Id });
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await PopulateFormSelectListsAsync(model, cancellationToken: cancellationToken);
            return View(model);
        }
    }

    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var teacher = await _teacherService.GetByIdAsync(id, cancellationToken);
            var model = ToFormModel(teacher);
            await PopulateFormSelectListsAsync(model, teacher, cancellationToken);
            return View(model);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, TeacherFormViewModel model, CancellationToken cancellationToken)
    {
        AddWhitespaceValidationErrors(model);
        if (!ModelState.IsValid)
        {
            model.Id = id;
            await PopulateFormSelectListsAsync(model, cancellationToken: cancellationToken);
            return View(model);
        }

        try
        {
            var teacher = await _teacherService.UpdateAsync(id, ToUpdateRequest(model), cancellationToken);
            return RedirectToAction(nameof(Details), new { id = teacher.Id });
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            model.Id = id;
            await PopulateFormSelectListsAsync(model, cancellationToken: cancellationToken);
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
            var teacher = await _teacherService.GetByIdAsync(id, cancellationToken);
            var model = await ToDetailsViewModelAsync(teacher, cancellationToken);
            return View(model);
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
            await _teacherService.DeleteAsync(id, cancellationToken);
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

    private async Task PopulateFormSelectListsAsync(TeacherFormViewModel model, TeacherResponse? existingTeacher = null, CancellationToken cancellationToken = default)
    {
        var languagesPage = await _offeredLanguageService.GetListAsync(
            new Application.Common.Requests.PageRequest { PageIndex = 0, PageSize = 100 },
            search: null, isActive: true, cancellationToken: cancellationToken);

        var branchesPage = await _branchService.GetListAsync(
            new Application.Common.Requests.PageRequest { PageIndex = 0, PageSize = 100 },
            search: null, isActive: true, cancellationToken: cancellationToken);

        model.AvailableLanguages = languagesPage.Items.Select(l =>
            new SelectListItem(l.Name, l.Id.ToString(),
                model.LanguageIds.Contains(l.Id))).ToList();

        model.AvailableBranches = branchesPage.Items.Select(b =>
            new SelectListItem(b.Name ?? "", b.Id.ToString(),
                model.BranchIds.Contains(b.Id))).ToList();

        if (existingTeacher is not null)
        {
            model.LanguageIds = existingTeacher.LanguageIds;
            model.BranchIds = existingTeacher.BranchIds;
            model.Availabilities = existingTeacher.Availabilities.Select(a =>
                new TeacherAvailabilityFormRow
                {
                    Id = a.Id,
                    DayOfWeek = a.DayOfWeek,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime
                }).ToList();
        }
    }

    private void AddWhitespaceValidationErrors(TeacherFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.FirstName))
            ModelState.AddModelError(nameof(model.FirstName), "Ad zorunludur.");
        if (string.IsNullOrWhiteSpace(model.LastName))
            ModelState.AddModelError(nameof(model.LastName), "Soyad zorunludur.");
        if (string.IsNullOrWhiteSpace(model.MobilePhone))
            ModelState.AddModelError(nameof(model.MobilePhone), "Cep telefonu zorunludur.");
    }

    private static TeacherFormViewModel ToFormModel(TeacherResponse teacher)
    {
        return new()
        {
            Id = teacher.Id,
            FirstName = teacher.FirstName,
            LastName = teacher.LastName,
            HomePhone = teacher.HomePhone,
            MobilePhone = teacher.MobilePhone,
            Email = teacher.Email,
            HireDate = teacher.HireDate,
            IsActive = teacher.IsActive,
            LanguageIds = teacher.LanguageIds,
            BranchIds = teacher.BranchIds,
            Availabilities = teacher.Availabilities.Select(a =>
                new TeacherAvailabilityFormRow
                {
                    Id = a.Id,
                    DayOfWeek = a.DayOfWeek,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime
                }).ToList()
        };
    }

    private async Task<TeacherDetailsViewModel> ToDetailsViewModelAsync(TeacherResponse teacher, CancellationToken cancellationToken)
    {
        var languagesPage = await _offeredLanguageService.GetListAsync(
            new Application.Common.Requests.PageRequest { PageIndex = 0, PageSize = 100 },
            search: null, isActive: true, cancellationToken: cancellationToken);
        var branchesPage = await _branchService.GetListAsync(
            new Application.Common.Requests.PageRequest { PageIndex = 0, PageSize = 100 },
            search: null, isActive: true, cancellationToken: cancellationToken);

        var languageMap = languagesPage.Items.ToDictionary(l => l.Id, l => l.Name);
        var branchMap = branchesPage.Items.ToDictionary(b => b.Id, b => b.Name ?? "");
        var dayNames = new[] { "Pazar", "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi" };

        return new TeacherDetailsViewModel
        {
            Id = teacher.Id,
            FirstName = teacher.FirstName,
            LastName = teacher.LastName,
            HomePhone = teacher.HomePhone,
            MobilePhone = teacher.MobilePhone,
            Email = teacher.Email,
            HireDate = teacher.HireDate,
            IsActive = teacher.IsActive,
            Languages = teacher.LanguageIds
                .Select(id => languageMap.TryGetValue(id, out var name) ? name : "Bilinmeyen")
                .ToList(),
            Branches = teacher.BranchIds
                .Select(id => branchMap.TryGetValue(id, out var name) ? name : "Bilinmeyen")
                .ToList(),
            Availabilities = teacher.Availabilities
                .Select(a => new TeacherAvailabilityDetailItem
                {
                    DayName = dayNames[(int)a.DayOfWeek],
                    StartTime = a.StartTime.ToString("HH:mm"),
                    EndTime = a.EndTime.ToString("HH:mm")
                }).ToList()
        };
    }

    private static CreateTeacherRequest ToCreateRequest(TeacherFormViewModel model)
    {
        return new()
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            HomePhone = model.HomePhone,
            MobilePhone = model.MobilePhone,
            Email = model.Email,
            HireDate = model.HireDate,
            LanguageIds = model.LanguageIds,
            BranchIds = model.BranchIds
        };
    }

    private static UpdateTeacherRequest ToUpdateRequest(TeacherFormViewModel model)
    {
        return new()
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            HomePhone = model.HomePhone,
            MobilePhone = model.MobilePhone,
            Email = model.Email,
            HireDate = model.HireDate,
            IsActive = model.IsActive,
            LanguageIds = model.LanguageIds,
            BranchIds = model.BranchIds
        };
    }
}
