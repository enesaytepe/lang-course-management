using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Classrooms;

namespace LanguageCourseManagement.Application.Services.ClassroomService;

public interface IClassroomService
{
    Task<ClassroomResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<GetListResponse<ClassroomListResponse>> GetListAsync(
        PageRequest pageRequest,
        string? search,
        Guid? branchId,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task<ClassroomResponse> CreateAsync(
        CreateClassroomRequest request,
        CancellationToken cancellationToken = default);

    Task<ClassroomResponse> UpdateAsync(
        Guid id,
        UpdateClassroomRequest request,
        CancellationToken cancellationToken = default);

    Task<ClassroomResponse> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}