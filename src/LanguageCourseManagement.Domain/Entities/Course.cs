using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.Domain.Entities;

/// <summary>
/// Açılan kurs
/// </summary>
public class Course : SoftDeletableEntity
{
    /// <summary>
    /// Dersin açıldığı şube Id
    /// </summary>
    public Guid BranchId { get; set; }

    /// <summary>
    /// Derste öğretilen dil Id
    /// </summary>
    public Guid OfferedLanguageId { get; set; }

    /// <summary>
    /// Dersin kurs seviyesi Id
    /// </summary>
    public Guid CourseLevelId { get; set; }

    /// <summary>
    /// Derse atanan öğretmen Id
    /// </summary>
    public Guid TeacherId { get; set; }

    /// <summary>
    /// Derse atanan derslik Id
    /// </summary>
    public Guid ClassroomId { get; set; }

    /// <summary>
    /// Ders adı
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Başlangıç tarihi
    /// </summary>
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Bitiş tarihi
    /// </summary>
    public DateOnly EndDate { get; set; }

    /// <summary>
    /// Ders kontenjanı
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Kurs ücreti
    /// </summary>
    public decimal TuitionFee { get; set; }

    /// <summary>
    /// Dersin kullanım durumu
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Dersin açılış durumu
    /// </summary>
    public CourseStatus Status { get; set; }

    /// <summary>
    /// Dersin açıldığı şube
    /// </summary>
    public virtual Branch Branch { get; set; } = null!;

    /// <summary>
    /// Derste öğretilen dil
    /// </summary>
    public virtual OfferedLanguage OfferedLanguage { get; set; } = null!;

    /// <summary>
    /// Dersin kurs seviyesi
    /// </summary>
    public virtual CourseLevel CourseLevel { get; set; } = null!;

    /// <summary>
    /// Derse atanan öğretmen
    /// </summary>
    public virtual Teacher Teacher { get; set; } = null!;

    /// <summary>
    /// Dersin yürütüldüğü derslik
    /// </summary>
    public virtual Classroom Classroom { get; set; } = null!;

    /// <summary>
    /// Dersin haftalık programı
    /// </summary>
    public virtual List<CourseSchedule>? Schedules { get; set; }

    /// <summary>
    /// Derse ait öğrenci kayıtları
    /// </summary>
    public virtual List<Enrollment>? Enrollments { get; set; }
}
