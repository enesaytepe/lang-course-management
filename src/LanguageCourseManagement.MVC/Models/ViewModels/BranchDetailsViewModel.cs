namespace LanguageCourseManagement.MVC.Models.ViewModels;

public sealed class BranchDetailsViewModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string? PublicTransportationDirections { get; init; }
    public string? PrivateVehicleDirections { get; init; }
    public string? PhoneNumber { get; init; }
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<string> Facilities { get; init; } = [];
}
