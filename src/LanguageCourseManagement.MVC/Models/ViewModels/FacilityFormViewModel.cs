using System.ComponentModel.DataAnnotations;

namespace LanguageCourseManagement.MVC.Models.ViewModels;

public sealed class FacilityFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Tesis adı zorunludur.")]
    [StringLength(200)]
    [Display(Name = "Tesis adı")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}
