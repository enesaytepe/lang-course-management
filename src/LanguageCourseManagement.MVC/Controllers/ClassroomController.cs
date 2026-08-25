using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.DTOs.Classrooms;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.BranchService;
using LanguageCourseManagement.Application.Services.ClassroomService;
using LanguageCourseManagement.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers;

public sealed class ClassroomController : Controller
{
    private readonly IClassroomService _classroomService;
    private readonly IBranchService _branchService;

    public ClassroomController(
        IClassroomService classroomService,
        IBranchService branchService)
    {
        _classroomService = classroomService;
        _branchService = branchService;
    }

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

    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ClassroomFormViewModel model,
        CancellationToken cancellationToken)
    {
        AddWhitespaceValidationError(model);

        if (!ModelState.IsValid)
        {
            await PopulateBranchesAsync(model, includeInactive: false, cancellationToken);
            return View(model);
        }

        try
        {
            var classroom = await _classroomService.CreateAsync(
                ToCreateRequest(model),
                cancellationToken);

            return RedirectToAction(nameof(Details), new { id = classroom.Id });
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await PopulateBranchesAsync(model, includeInactive: false, cancellationToken);
            return View(model);
        }
    }

    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Edit(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var classroom = await _classroomService.GetByIdAsync(id, cancellationToken);
            var model = ToFormModel(classroom);
            await PopulateBranchesAsync(model, includeInactive: true, cancellationToken);
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
    public async Task<IActionResult> Edit(
        Guid id,
        ClassroomFormViewModel model,
        CancellationToken cancellationToken)
    {
        AddWhitespaceValidationError(model);

        if (!ModelState.IsValid)
        {
            model.Id = id;
            await PopulateBranchesAsync(model, includeInactive: true, cancellationToken);
            return View(model);
        }

        try
        {
            var classroom = await _classroomService.UpdateAsync(
                id,
                ToUpdateRequest(model),
                cancellationToken);

            return RedirectToAction(nameof(Details), new { id = classroom.Id });
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            model.Id = id;
            await PopulateBranchesAsync(model, includeInactive: true, cancellationToken);
            return View(model);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet]
    [Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
    public async Task<IActionResult> Details(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            return View(await _classroomService.GetByIdAsync(id, cancellationToken));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    private async Task<ClassroomFormViewModel> CreateFormModelAsync(
        CancellationToken cancellationToken)
    {
        var model = new ClassroomFormViewModel();
        await PopulateBranchesAsync(model, includeInactive: false, cancellationToken);
        return model;
    }

    private async Task PopulateBranchesAsync(
        ClassroomFormViewModel model,
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        const int pageSize = 100;
        var pageIndex = 0;
        var options = new List<ClassroomBranchOptionViewModel>();

        while (true)
        {
            var branches = await _branchService.GetListAsync(
                new PageRequest { PageIndex = pageIndex, PageSize = pageSize },
                search: null,
                isActive: includeInactive ? null : true,
                cancellationToken);

            options.AddRange(branches.Items.Select(branch => new ClassroomBranchOptionViewModel
            {
                Id = branch.Id,
                Name = branch.Name ?? string.Empty,
                IsActive = branch.IsActive
            }));

            if (!branches.HasNext)
                break;

            pageIndex++;
        }

        model.Branches = options
            .OrderBy(branch => branch.Name)
            .ToList();
    }

    private void AddWhitespaceValidationError(ClassroomFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError(nameof(model.Name), "Derslik adı zorunludur.");
    }

    private static ClassroomFormViewModel ToFormModel(ClassroomResponse classroom)
    {
        return new()
        {
            Id = classroom.Id,
            BranchId = classroom.BranchId,
            Name = classroom.Name,
            Description = classroom.Description,
            Capacity = classroom.Capacity,
            IsActive = classroom.IsActive
        };
    }

    private static CreateClassroomRequest ToCreateRequest(
        ClassroomFormViewModel model)
    {
        return new()
        {
            BranchId = model.BranchId.GetValueOrDefault(),
            Name = model.Name,
            Description = model.Description,
            Capacity = model.Capacity
        };
    }

    private static UpdateClassroomRequest ToUpdateRequest(
        ClassroomFormViewModel model)
    {
        return new()
        {
            BranchId = model.BranchId.GetValueOrDefault(),
            Name = model.Name,
            Description = model.Description,
            Capacity = model.Capacity,
            IsActive = model.IsActive
        };
    }
}