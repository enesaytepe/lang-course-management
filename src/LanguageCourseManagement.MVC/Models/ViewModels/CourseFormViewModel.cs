using System.ComponentModel.DataAnnotations;
using LanguageCourseManagement.Application.DTOs.Courses;
using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.MVC.Models.ViewModels;

public sealed class CourseFormViewModel
{
    public Guid? Id { get; set; }
    [Required(ErrorMessage = "Ders adı zorunludur.")][StringLength(150)][Display(Name = "Ders adı")]
    public string Name { get; set; } = string.Empty;
    [Required(ErrorMessage = "Şube seçimi zorunludur.")][Display(Name = "Şube")]
    public Guid? BranchId { get; set; }
    [Required(ErrorMessage = "Dil seçimi zorunludur.")][Display(Name = "Dil")]
    public Guid? OfferedLanguageId { get; set; }
    [Required(ErrorMessage = "Seviye seçimi zorunludur.")][Display(Name = "Seviye")]
    public Guid? CourseLevelId { get; set; }
    [Required(ErrorMessage = "Öğretmen seçimi zorunludur.")][Display(Name = "Öğretmen")]
    public Guid? TeacherId { get; set; }
    [Required(ErrorMessage = "Derslik seçimi zorunludur.")][Display(Name = "Derslik")]
    public Guid? ClassroomId { get; set; }
    [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")][Display(Name = "Başlangıç tarihi")]
    public DateOnly? StartDate { get; set; }
    [Required(ErrorMessage = "Bitiş tarihi zorunludur.")][Display(Name = "Bitiş tarihi")]
    public DateOnly? EndDate { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Kontenjan 1 veya daha büyük olmalıdır.")][Display(Name = "Kontenjan")]
    public int Capacity { get; set; } = 1;
    [Range(0, double.MaxValue, ErrorMessage = "Kurs ücreti negatif olamaz.")][Display(Name = "Kurs ücreti (₺)")]
    public decimal TuitionFee { get; set; }
    [Display(Name = "Durum")] public CourseStatus Status { get; set; } = CourseStatus.Draft;
    public bool IsActive { get; set; } = true;
    public List<CourseScheduleItemDto> Schedules { get; set; } = [];
    public IReadOnlyList<CourseBranchOptionViewModel> Branches { get; set; } = [];
    public IReadOnlyList<CourseLanguageOptionViewModel> Languages { get; set; } = [];
    public IReadOnlyList<CourseLevelOptionViewModel> Levels { get; set; } = [];
    public IReadOnlyList<EligibleTeacherResponse> EligibleTeachers { get; set; } = [];
    public IReadOnlyList<EligibleClassroomResponse> EligibleClassrooms { get; set; } = [];
}

public sealed class CourseBranchOptionViewModel { public Guid Id { get; init; } public string Name { get; init; } = string.Empty; public bool IsActive { get; init; } }
public sealed class CourseLanguageOptionViewModel { public Guid Id { get; init; } public string Name { get; init; } = string.Empty; public bool IsActive { get; init; } }
public sealed class CourseLevelOptionViewModel { public Guid Id { get; init; } public string Name { get; init; } = string.Empty; public bool IsActive { get; init; } }
