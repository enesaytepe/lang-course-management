using System.ComponentModel.DataAnnotations;
using LanguageCourseManagement.Domain.Enums;

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
    /// Tahsilat yapılacak taksit Id (nakit ödemelerde null)
    /// </summary>
    public Guid? InstallmentId { get; set; }

    /// <summary>
    /// Tahsilat yöntemi
    /// </summary>
    [Display(Name = "Yöntem")]
    public PaymentMethod Method { get; set; } = PaymentMethod.Cash;

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
    public string EnrollmentPaymentType { get; set; } = string.Empty;

    /// <summary>
    /// Henüz tahsilat yapılmamış aktif kayıtlar (dropdown için)
    /// </summary>
    public IReadOnlyList<EnrollmentOptionViewModel> UnsettledEnrollments { get; set; } = [];

    /// <summary>
    /// Kayıtlara ait taksit seçenekleri (taksitli ödemeler için)
    /// </summary>
    public Dictionary<Guid, List<InstallmentOptionViewModel>> EnrollmentInstallments { get; set; } = new();
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
    public string PaymentType { get; init; } = string.Empty;
}

/// <summary>
/// Taksit seçeneği için bilgi.
/// </summary>
public sealed class InstallmentOptionViewModel
{
    public Guid Id { get; init; }
    public int InstallmentNumber { get; init; }
    public decimal Amount { get; init; }
    public DateOnly DueDate { get; init; }
    public string Status { get; init; } = string.Empty;
}
