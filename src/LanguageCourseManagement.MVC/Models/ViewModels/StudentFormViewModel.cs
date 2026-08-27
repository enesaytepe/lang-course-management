using System.ComponentModel.DataAnnotations;

namespace LanguageCourseManagement.MVC.Models.ViewModels;

public sealed class StudentFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Ad zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Ad")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Soyad")]
    public string LastName { get; set; } = string.Empty;

    [StringLength(20)]
    [Display(Name = "Ev telefonu")]
    [RegularExpression("^[0-9+\\s()\\-]*$", ErrorMessage = "Telefon numarası geçersiz.")]
    public string? HomePhone { get; set; }

    [Required(ErrorMessage = "Cep telefonu zorunludur.")]
    [StringLength(20)]
    [Display(Name = "Cep telefonu")]
    [RegularExpression("^[0-9+\\s()\\-]*$", ErrorMessage = "Telefon numarası geçersiz.")]
    public string MobilePhone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    [Display(Name = "E-posta")]
    public string? Email { get; set; }

    [StringLength(500)]
    [Display(Name = "Adres")]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;
}
