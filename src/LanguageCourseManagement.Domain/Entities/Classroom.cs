namespace LanguageCourseManagement.Domain.Entities;

/// <summary>
/// Kurs dersliği
/// </summary>
public class Classroom : SoftDeletableEntity
{
    /// <summary>
    /// Dersliğin bağlı olduğu şube Id
    /// </summary>
    public Guid BranchId { get; set; }

    /// <summary>
    /// Derslik adı
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Derslik açıklaması
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Kapasite
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Dersliğin kullanım durumu
    /// </summary>
    public bool IsActive { get; set; }


    /// <summary>
    /// Dersliğin bağlı olduğu şube
    /// </summary>
    public virtual Branch Branch { get; set; } = null!;

    /// <summary>
    /// Derslikte planlanan dersler
    /// </summary>
    public virtual List<Course>? Courses { get; set; }
}
