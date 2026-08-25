using System.ComponentModel.DataAnnotations;

namespace LanguageCourseManagement.MVC.Models.ViewModels;

public sealed class LanguageFormViewModel
{
    public Guid? Id { get; set; }
    [Required(ErrorMessage = "Dil adı zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Dil adı")]
    public string Name { get; set; } = string.Empty;
    [StringLength(10)]
    [Display(Name = "Kod")]
    public string? Code { get; set; }
    public bool IsActive { get; set; } = true;
}
