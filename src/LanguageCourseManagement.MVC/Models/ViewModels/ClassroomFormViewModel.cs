using System.ComponentModel.DataAnnotations;

namespace LanguageCourseManagement.MVC.Models.ViewModels;

public sealed class ClassroomFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Şube seçimi zorunludur.")]
    [Display(Name = "Şube")]
    public Guid? BranchId { get; set; }

    [Required(ErrorMessage = "Derslik adı zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Derslik adı")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Kapasite 1 veya daha büyük olmalıdır.")]
    [Display(Name = "Kapasite")]
    public int Capacity { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public IReadOnlyList<ClassroomBranchOptionViewModel> Branches { get; set; } = [];
}

public sealed class ClassroomBranchOptionViewModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}