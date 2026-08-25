using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Courses;

namespace LanguageCourseManagement.Application.Services.CourseService;

/// <summary>
/// Ders işlemlerini tanımlar.
/// </summary>
public interface ICourseService
{
    Task<CourseResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<GetListResponse<CourseListResponse>> GetListAsync(
        PageRequest pageRequest,
        string? search,
        Guid? branchId,
        Guid? offeredLanguageId,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task<CourseResponse> CreateAsync(CreateCourseRequest request, CancellationToken cancellationToken = default);

    Task<CourseResponse> UpdateAsync(Guid id, UpdateCourseRequest request, CancellationToken cancellationToken = default);

    Task<CourseResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CourseScheduleItemDto>> GetSchedulesAsync(Guid courseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EligibleTeacherResponse>> GetEligibleTeachersAsync(
        Guid branchId, Guid offeredLanguageId, Guid courseLevelId,
        DateOnly startDate, DateOnly endDate,
        IReadOnlyList<CourseScheduleItemDto> schedules,
        Guid? excludeCourseId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EligibleClassroomResponse>> GetEligibleClassroomsAsync(
        Guid branchId, DateOnly startDate, DateOnly endDate,
        IReadOnlyList<CourseScheduleItemDto> schedules,
        Guid? excludeCourseId = null, CancellationToken cancellationToken = default);
}
