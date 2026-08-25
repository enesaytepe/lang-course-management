using System.ComponentModel.DataAnnotations;

namespace LanguageCourseManagement.MVC.Models.ViewModels;

public sealed class BranchFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Şube adı zorunludur.")]
    [StringLength(200)]
    [Display(Name = "Şube adı")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adres zorunludur.")]
    [StringLength(500)]
    [Display(Name = "Adres")]
    public string Address { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? PublicTransportationDirections { get; set; }

    [StringLength(1000)]
    public string? PrivateVehicleDirections { get; set; }

    [Range(-90, 90, ErrorMessage = "Enlem -90 ile 90 arasında olmalıdır.")]
    public decimal Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Boylam -180 ile 180 arasında olmalıdır.")]
    public decimal Longitude { get; set; }

    [StringLength(32)]
    [RegularExpression("^[0-9+\\s()\\-]*$", ErrorMessage = "Telefon numarası geçersiz karakterler içeriyor.")]
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Guid> FacilityIds { get; set; } = [];
    public IReadOnlyList<FacilityOptionViewModel> Facilities { get; set; } = [];
}

public sealed class FacilityOptionViewModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}
