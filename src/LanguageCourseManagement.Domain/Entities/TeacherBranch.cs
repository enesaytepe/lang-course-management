namespace LanguageCourseManagement.Domain.Entities;

/// <summary>
/// Öğretmen şube ilişkisi
/// </summary>
public class TeacherBranch : BaseEntity
{
    /// <summary>
    /// Şubede ders verebilen öğretmen Id
    /// </summary>
    public Guid TeacherId { get; set; }

    /// <summary>
    /// Öğretmenin ders verebildiği şube Id
    /// </summary>
    public Guid BranchId { get; set; }

    /// <summary>
    /// Şubede ders verebilen öğretmen
    /// </summary>
    public Teacher Teacher { get; set; } = null!;

    /// <summary>
    /// Öğretmenin ders verebildiği şube
    /// </summary>
    public Branch Branch { get; set; } = null!;
}
