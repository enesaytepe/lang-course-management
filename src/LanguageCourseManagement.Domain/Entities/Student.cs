namespace LanguageCourseManagement.Domain.Entities;

/// <summary>
/// Kursa kayıtlı öğrenci
/// </summary>
public class Student : SoftDeletableEntity
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
    /// Adres
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Öğrencinin sisteme ilk kayıt tarihi
    /// </summary>
    public DateTime RegistrationDate { get; set; }

    /// <summary>
    /// Öğrencinin kayıt durumu
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Öğrencinin ders kayıtları
    /// </summary>
    public List<Enrollment>? Enrollments { get; set; }
}
