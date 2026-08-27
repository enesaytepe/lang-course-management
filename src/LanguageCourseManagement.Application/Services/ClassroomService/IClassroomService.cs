using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Classrooms;
using LanguageCourseManagement.Application.DTOs.Schedules;

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
        bool showDeleted = false,
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

    Task<List<WeeklyScheduleResponse>> GetWeeklyScheduleAsync(
        Guid classroomId,
        CancellationToken cancellationToken = default);
}