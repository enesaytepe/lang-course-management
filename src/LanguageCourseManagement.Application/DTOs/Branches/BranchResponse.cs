namespace LanguageCourseManagement.Application.DTOs.Branches;

/// <summary>
/// Tek şube detay bilgisini içeren yanıt.
/// </summary>
public class BranchResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? PublicTransportationDirections { get; set; }
    public string? PrivateVehicleDirections { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyCollection<Guid> FacilityIds { get; set; } = [];
}
