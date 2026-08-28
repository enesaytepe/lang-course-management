using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Students;

namespace LanguageCourseManagement.Application.Services.StudentService;

/// <summary>
/// Öğrenci işlemlerini tanımlar.
/// </summary>
public interface IStudentService
{
    /// <summary>
    /// ID'ye göre öğrenciyi getirir.
    /// </summary>
    Task<StudentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Arama ve filtre kriterlerine göre öğrencileri sayfalamalı getirir.
    /// </summary>
    Task<GetListResponse<StudentListResponse>> GetListAsync(PageRequest pageRequest, string? search, bool? isActive, bool showDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Yeni öğrenci oluşturur.
    /// </summary>
    Task<StudentResponse> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mevcut öğrenciyi günceller.
    /// </summary>
    Task<StudentResponse> UpdateAsync(Guid id, UpdateStudentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Öğrenciyi soft delete ile siler.
    /// </summary>
    Task<StudentResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
