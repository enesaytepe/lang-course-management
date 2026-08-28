namespace LanguageCourseManagement.Domain.Entities;

/// <summary>
/// Kurs öğretmeni
/// </summary>
public class Teacher : SoftDeletableEntity
{
    /// <summary>
    /// Ad
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    /// <summary>
    /// Soyad
    /// </summary>
    public string LastName { get; set; } = string.Empty;
    /// <summary>
    /// Ev telefonu
    /// </summary>
    public string? HomePhone { get; set; }
    /// <summary>
    /// Cep telefonu
    /// </summary>
    public string MobilePhone { get; set; } = string.Empty;
    /// <summary>
    /// E-posta adresi
    /// </summary>
    public string? Email { get; set; }
    /// <summary>
    /// İşe başlama tarihi
    /// </summary>
    public DateOnly HireDate { get; set; }
    /// <summary>
    /// Öğretmenin ders verebilme durumu
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Öğretmenin öğretebildiği diller
    /// </summary>
    public List<TeacherLanguage>? TeacherLanguages { get; set; }
    /// <summary>
    /// Öğretmenin ders verebildiği şubeler
    /// </summary>
    public List<TeacherBranch>? TeacherBranches { get; set; }
    /// <summary>
    /// Öğretmenin haftalık müsaitlik zamanları
    /// </summary>
    public List<TeacherAvailability>? Availabilities { get; set; }
    /// <summary>
    /// Öğretmene atanan dersler
    /// </summary>
    public List<Course>? Courses { get; set; }
    /// <summary>
    /// Öğretmenin öğretebildiği kurs seviyeleri
    /// </summary>
    public List<TeacherCourseLevel>? TeacherCourseLevels { get; set; }
}
