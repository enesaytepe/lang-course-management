namespace LanguageCourseManagement.Domain.Entities;

/// <summary>
/// Kurs şubesi
/// </summary>
public class Branch : SoftDeletableEntity
{
    /// <summary>
    /// Şube adı
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Adres
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Toplu taşıma ulaşım bilgisi
    /// </summary>
    public string? PublicTransportationDirections { get; set; }

    /// <summary>
    /// Özel araç ulaşım bilgisi
    /// </summary>
    public string? PrivateVehicleDirections { get; set; }

    /// <summary>
    /// Enlem
    /// </summary>
    public decimal Latitude { get; set; }

    /// <summary>
    /// Boylam
    /// </summary>
    public decimal Longitude { get; set; }

    /// <summary>
    /// Telefon numarası
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Şubenin kullanım durumu
    /// </summary>
    public bool IsActive { get; set; }


    /// <summary>
    /// Şubede sunulan olanak ilişkileri
    /// </summary>
    public virtual List<BranchFacility>? BranchFacilities { get; set; }

    /// <summary>
    /// Şubeye ait derslikler
    /// </summary>
    public virtual List<Classroom>? Classrooms { get; set; }

    /// <summary>
    /// Şubede ders verebilen öğretmen ilişkileri
    /// </summary>
    public virtual List<TeacherBranch>? TeacherBranches { get; set; }

    /// <summary>
    /// Şubede açılan dersler
    /// </summary>
    public virtual List<Course>? Courses { get; set; }
}