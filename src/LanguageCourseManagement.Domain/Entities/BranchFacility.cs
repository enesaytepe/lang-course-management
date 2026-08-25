namespace LanguageCourseManagement.Domain.Entities;

/// <summary>
/// Şube olanağı ilişkisi
/// </summary>
public class BranchFacility : BaseEntity
{
    /// <summary>
    /// Olanağın sunulduğu şube Id
    /// </summary>
    public Guid BranchId { get; set; }

    /// <summary>
    /// Şubede sunulan olanak Id
    /// </summary>
    public Guid FacilityId { get; set; }


    /// <summary>
    /// Olanağın sunulduğu şube
    /// </summary>
    public virtual Branch Branch { get; set; } = null!;

    /// <summary>
    /// Şubede sunulan olanak
    /// </summary>
    public virtual Facility Facility { get; set; } = null!;
}
