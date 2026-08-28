using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LanguageCourseManagement.MVC.Models.ViewModels;

public sealed class TeacherFormViewModel
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

    [Required(ErrorMessage = "İşe başlama tarihi zorunludur.")]
    [Display(Name = "İşe başlama tarihi")]
    public DateOnly HireDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public bool IsActive { get; set; } = true;

    [Display(Name = "Diller")]
    public List<Guid> LanguageIds { get; set; } = [];

    [Display(Name = "Şubeler")]
    public List<Guid> BranchIds { get; set; } = [];

    [Display(Name = "Kurs Seviyeleri")]
    public List<Guid> CourseLevelIds { get; set; } = [];

    public List<SelectListItem> AvailableLanguages { get; set; } = [];

    public List<SelectListItem> AvailableBranches { get; set; } = [];

    public List<SelectListItem> AvailableCourseLevels { get; set; } = [];

    public List<TeacherAvailabilityFormRow> Availabilities { get; set; } = [];
}

public sealed class TeacherAvailabilityFormRow
{
    public Guid? Id { get; set; }

    [Display(Name = "Gün")]
    public DayOfWeek DayOfWeek { get; set; }

    [Display(Name = "Başlangıç")]
    public TimeOnly StartTime { get; set; } = new(9, 0);

    [Display(Name = "Bitiş")]
    public TimeOnly EndTime { get; set; } = new(17, 0);
}
