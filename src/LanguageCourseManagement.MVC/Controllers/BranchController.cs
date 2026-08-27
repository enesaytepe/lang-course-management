using LanguageCourseManagement.Application.DTOs.Branches;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.BranchService;
using LanguageCourseManagement.Application.Services.FacilityService;
using LanguageCourseManagement.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers;

/// <summary>
/// Şube yönetimi endpoint'leri
/// </summary>
public sealed class BranchController : Controller
{
    private readonly IBranchService _branchService;
    private readonly IFacilityService _facilityService;

    public BranchController(IBranchService branchService, IFacilityService facilityService)
    {
        _branchService = branchService;
        _facilityService = facilityService;
    }

    /// <summary>
    /// Şube listesini görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
    public async Task<IActionResult> Index(bool useCreateModal = true, CancellationToken cancellationToken = default)
    {
        ViewData["UseCreateModal"] = useCreateModal;
        return View(await CreateFormModelAsync(cancellationToken));
    }

    /// <summary>
    /// Yeni şube oluşturma ekranını görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        return View(await CreateFormModelAsync(cancellationToken));
    }

    /// <summary>
    /// Şube düzenleme ekranını görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var branch = await _branchService.GetByIdAsync(id, cancellationToken);
            var model = ToFormModel(branch);
            await PopulateFacilitiesAsync(model, includeInactive: true, cancellationToken);
            return View(model);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Şube detaylarını görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var details = await _branchService.GetDetailsAsync(id, cancellationToken);

            return View(new BranchDetailsViewModel
            {
                Id = details.Id,
                Name = details.Name,
                Address = details.Address,
                PublicTransportationDirections = details.PublicTransportationDirections,
                PrivateVehicleDirections = details.PrivateVehicleDirections,
                PhoneNumber = details.PhoneNumber,
                Latitude = details.Latitude,
                Longitude = details.Longitude,
                IsActive = details.IsActive,
                Facilities = details.FacilityNames,
                Classrooms = details.Classrooms.Select(c => new BranchDetailsClassroomItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    Capacity = c.Capacity,
                    IsActive = c.IsActive
                }).ToList(),
                Courses = details.Courses.Select(c => new BranchDetailsCourseItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    LevelName = c.LevelName,
                    TeacherName = c.TeacherName,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    StatusText = c.Status switch
                    {
                        Domain.Enums.CourseStatus.Draft => "Taslak",
                        Domain.Enums.CourseStatus.Open => "Açık",
                        Domain.Enums.CourseStatus.Completed => "Tamamlandı",
                        Domain.Enums.CourseStatus.Cancelled => "İptal",
                        _ => c.Status.ToString()
                    },
                    IsActive = c.IsActive
                }).ToList(),
                Teachers = details.Teachers.Select(t => new BranchDetailsTeacherItem
                {
                    Id = t.Id,
                    FirstName = t.FirstName,
                    LastName = t.LastName,
                    MobilePhone = t.MobilePhone,
                    Languages = t.Languages,
                    IsActive = t.IsActive
                }).ToList()
            });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    private async Task<BranchFormViewModel> CreateFormModelAsync(CancellationToken cancellationToken)
    {
        var model = new BranchFormViewModel();
        await PopulateFacilitiesAsync(model, includeInactive: false, cancellationToken);
        return model;
    }

    private async Task PopulateFacilitiesAsync(
        BranchFormViewModel model,
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        var facilities = includeInactive
            ? await _facilityService.GetAllAsync(cancellationToken)
            : await _facilityService.GetActiveAsync(cancellationToken);
        model.Facilities = facilities
            .Select(facility => new FacilityOptionViewModel { Id = facility.Id, Name = facility.Name, IsActive = facility.IsActive })
            .ToList();
    }

    private static BranchFormViewModel ToFormModel(BranchResponse branch)
    {
        return new()
        {
            Id = branch.Id,
            Name = branch.Name ?? string.Empty,
            Address = branch.Address ?? string.Empty,
            PublicTransportationDirections = branch.PublicTransportationDirections,
            PrivateVehicleDirections = branch.PrivateVehicleDirections,
            PhoneNumber = branch.PhoneNumber,
            Latitude = branch.Latitude,
            Longitude = branch.Longitude,
            IsActive = branch.IsActive,
            FacilityIds = branch.FacilityIds.ToList()
        };
    }
}
