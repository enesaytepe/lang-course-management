using System.ComponentModel.DataAnnotations;

namespace LanguageCourseManagement.MVC.Models.ViewModels;

/// <summary>
/// Tahsilat oluşturma formu view modeli.
/// </summary>
public sealed class PaymentFormViewModel
{
    /// <summary>
    /// Tahsilat yapılacak kayıt Id
    /// </summary>
    [Required(ErrorMessage = "Kayıt seçimi zorunludur.")]
    [Display(Name = "Kayıt")]
    public Guid EnrollmentId { get; set; }

    /// <summary>
    /// Tahsilat açıklaması (isteğe bağlı)
    /// </summary>
    [StringLength(500)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    // Seçilen kaydın bilgileri (display only)
    public string StudentName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public decimal FinalAmount { get; set; }

    /// <summary>
    /// Henüz tahsilat yapılmamış aktif kayıtlar (dropdown için)
    /// </summary>
    public IReadOnlyList<EnrollmentOptionViewModel> UnsettledEnrollments { get; set; } = [];
}

/// <summary>
/// Dropdown seçeneği için kayıt özeti.
/// </summary>
public sealed class EnrollmentOptionViewModel
{
    public Guid Id { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string CourseName { get; init; } = string.Empty;
    public string BranchName { get; init; } = string.Empty;
    public decimal FinalAmount { get; init; }
}
