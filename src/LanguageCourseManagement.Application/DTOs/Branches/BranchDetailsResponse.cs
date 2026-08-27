using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.Application.DTOs.Branches;

/// <summary>
/// Şube detay bilgileri: şube bilgileri, derslikler, kurslar ve öğretmenler dahil.
/// </summary>
public sealed class BranchDetailsResponse
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
    public IReadOnlyList<string> FacilityNames { get; init; } = [];
    public IReadOnlyList<BranchClassroomItem> Classrooms { get; init; } = [];
    public IReadOnlyList<BranchCourseItem> Courses { get; init; } = [];
    public IReadOnlyList<BranchTeacherItem> Teachers { get; init; } = [];
}

public sealed class BranchClassroomItem
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Capacity { get; init; }
    public bool IsActive { get; init; }
}

public sealed class BranchCourseItem
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string LevelName { get; init; } = string.Empty;
    public string TeacherName { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public CourseStatus Status { get; init; }
    public bool IsActive { get; init; }
}

public sealed class BranchTeacherItem
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string MobilePhone { get; init; } = string.Empty;
    public IReadOnlyList<string> Languages { get; init; } = [];
    public bool IsActive { get; init; }
}
