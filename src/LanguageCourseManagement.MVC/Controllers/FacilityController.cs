using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Services.FacilityService;
using LanguageCourseManagement.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers;

/// <summary>
/// Global facility catalog screens. Mutations are owned by the Facility API.
/// </summary>
[Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
public sealed class FacilityController : Controller
{
    private readonly IFacilityService _facilityService;

    public FacilityController(IFacilityService facilityService)
    {
        _facilityService = facilityService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewData["FacilityListEndpoint"] = "/api/facilities/crud-list";
        ViewData["FacilityEndpoint"] = "/api/facilities";
        return View(new FacilityFormViewModel());
    }

    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public IActionResult Create()
    {
        return View(new FacilityFormViewModel());
    }

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
