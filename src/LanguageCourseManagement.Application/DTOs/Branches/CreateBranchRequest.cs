namespace LanguageCourseManagement.Application.DTOs.Branches;

public sealed class CreateBranchRequest
{
    public required string Name { get; set; }
    public required string Address { get; set; }
    public string? PublicTransportationDirections { get; set; }
    public string? PrivateVehicleDirections { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? PhoneNumber { get; set; }
    public IReadOnlyCollection<Guid>? FacilityIds { get; init; }
}
