namespace LanguageCourseManagement.Domain.Entities;

/// <summary>
/// Şube olanağı
/// </summary>
public class Facility : SoftDeletableEntity
{
    /// <summary>
    /// Olanak adı
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Olanak açıklaması
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Olanağın kullanım durumu
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Olanağın sunulduğu şube ilişkileri
    /// </summary>
    public virtual List<BranchFacility>? BranchFacilities { get; set; }
}