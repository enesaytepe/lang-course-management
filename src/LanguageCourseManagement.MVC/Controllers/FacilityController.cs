using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.FacilityService;
using LanguageCourseManagement.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers;

/// <summary>
/// Tesis yönetimi endpoint'leri.
/// </summary>
[Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
public sealed class FacilityController : Controller
{
    private readonly IFacilityService _facilityService;

    public FacilityController(IFacilityService facilityService)
    {
        _facilityService = facilityService;
    }

    /// <summary>
    /// Tesis listesini görüntüler.
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["FacilityListEndpoint"] = "/api/facilities/crud-list";
        ViewData["FacilityEndpoint"] = "/api/facilities";
        return View(new FacilityFormViewModel());
    }

    /// <summary>
    /// Yeni tesis oluşturma ekranını görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public IActionResult Create()
    {
        return View(new FacilityFormViewModel());
    }

    /// <summary>
    /// Tesis düzenleme ekranını görüntüler.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var facility = await _facilityService.GetByIdAsync(id, cancellationToken);
            return View(new FacilityFormViewModel
            {
                Id = facility.Id,
                Name = facility.Name,
                Description = facility.Description,
                IsActive = facility.IsActive
            });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Tesis detaylarını görüntüler.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var facility = await _facilityService.GetByIdAsync(id, cancellationToken);
            return View(new FacilityDetailsViewModel
            {
                Id = facility.Id,
                Name = facility.Name,
                Description = facility.Description,
                IsActive = facility.IsActive
            });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
