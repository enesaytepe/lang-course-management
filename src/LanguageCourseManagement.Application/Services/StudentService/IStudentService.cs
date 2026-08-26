using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Students;

namespace LanguageCourseManagement.Application.Services.StudentService;

/// <summary>
/// Öğrenci işlemlerini tanımlar.
/// </summary>
public interface IStudentService
{
    Task<StudentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetListResponse<StudentListResponse>> GetListAsync(PageRequest pageRequest, string? search, bool? isActive, bool showDeleted = false, CancellationToken cancellationToken = default);
    Task<StudentResponse> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default);
    Task<StudentResponse> UpdateAsync(Guid id, UpdateStudentRequest request, CancellationToken cancellationToken = default);
    Task<StudentResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
