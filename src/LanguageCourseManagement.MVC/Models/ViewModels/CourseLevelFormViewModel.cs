using System.ComponentModel.DataAnnotations;

namespace LanguageCourseManagement.MVC.Models.ViewModels;

public sealed class CourseLevelFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Dil seçimi zorunludur.")]
    [Display(Name = "Dil")]
    public Guid? OfferedLanguageId { get; set; }

    [Required(ErrorMessage = "Seviye adı zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Seviye adı")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Sıra numarası negatif olamaz.")]
    [Display(Name = "Sıra")]
    public int Order { get; set; }

    public bool IsActive { get; set; } = true;

    public IReadOnlyList<CourseLevelLanguageOptionViewModel> Languages { get; set; } = [];
}

public sealed class CourseLevelLanguageOptionViewModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}
