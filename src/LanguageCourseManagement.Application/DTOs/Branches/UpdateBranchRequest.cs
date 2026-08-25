namespace LanguageCourseManagement.Application.DTOs.Branches;

public sealed class UpdateBranchRequest
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? PublicTransportationDirections { get; set; }
    public string? PrivateVehicleDirections { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public List<Guid>? FacilityIds { get; set; }
}
