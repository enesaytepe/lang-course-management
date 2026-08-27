using AutoMapper;
using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.DTOs.Classrooms;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.BranchService;
using LanguageCourseManagement.Application.Services.ClassroomService;
using LanguageCourseManagement.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers;

/// <summary>
/// Derslik yönetimi endpoint'leri.
/// </summary>
public sealed class ClassroomController : Controller
{
    private readonly IClassroomService _classroomService;
    private readonly IBranchService _branchService;
    private readonly IMapper _mapper;

    public ClassroomController(
        IClassroomService classroomService,
        IBranchService branchService,
        IMapper mapper)
    {
        _classroomService = classroomService;
        _branchService = branchService;
        _mapper = mapper;
    }

    /// <summary>
    /// Derslik listesini görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await CreateFormModelAsync(cancellationToken));
    }

    /// <summary>
    /// Yeni derslik oluşturma ekranını görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        return View(await CreateFormModelAsync(cancellationToken));
    }

    /// <summary>
    /// Yeni derslik oluşturur.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ClassroomFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateBranchesAsync(model, includeInactive: false, cancellationToken);
            return View(model);
        }

        try
        {
            var classroom = await _classroomService.CreateAsync(
                _mapper.Map<CreateClassroomRequest>(model),
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

    /// <summary>
    /// Derslik düzenleme ekranını görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Edit(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var classroom = await _classroomService.GetByIdAsync(id, cancellationToken);
            var model = _mapper.Map<ClassroomFormViewModel>(classroom);
            await PopulateBranchesAsync(model, includeInactive: true, cancellationToken);
            return View(model);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Derslik bilgilerini günceller.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid id,
        ClassroomFormViewModel model,
        CancellationToken cancellationToken)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            await PopulateBranchesAsync(model, includeInactive: true, cancellationToken);
            return View(model);
        }

        try
        {
            var classroom = await _classroomService.UpdateAsync(
                id,
                _mapper.Map<UpdateClassroomRequest>(model),
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

    /// <summary>
    /// Derslik detaylarını görüntüler.
    /// </summary>
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
                cancellationToken: cancellationToken);

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

}
