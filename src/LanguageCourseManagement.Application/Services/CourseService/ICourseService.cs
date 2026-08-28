using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Courses;
using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.Application.Services.CourseService;

/// <summary>
/// Ders işlemlerini tanımlar.
/// </summary>
public interface ICourseService
{
    /// <summary>
    /// ID'ye göre dersi getirir.
    /// </summary>
    Task<CourseResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Arama ve filtre kriterlerine göre dersleri sayfalamalı getirir.
    /// </summary>
    Task<GetListResponse<CourseListResponse>> GetListAsync(
        PageRequest pageRequest,
        string? search,
        Guid? branchId,
        Guid? offeredLanguageId,
        bool? isActive,
        CourseStatus? status = null,
        bool showDeleted = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Yeni ders oluşturur.
    /// </summary>
    Task<CourseResponse> CreateAsync(CreateCourseRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mevcut dersi günceller.
    /// </summary>
    Task<CourseResponse> UpdateAsync(Guid id, UpdateCourseRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dersi soft delete ile siler.
    /// </summary>
    Task<CourseResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Derse ait ders programı (schedule) kayıtlarını getirir.
    /// </summary>
    Task<IReadOnlyList<CourseScheduleItemDto>> GetSchedulesAsync(Guid courseId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Verilen tarih aralığı ve program bilgisine göre uygun öğretmenleri listeler.
    /// </summary>
    Task<IReadOnlyList<EligibleTeacherResponse>> GetEligibleTeachersAsync(
        Guid branchId, Guid offeredLanguageId, Guid courseLevelId,
        DateOnly startDate, DateOnly endDate,
        IReadOnlyList<CourseScheduleItemDto> schedules,
        Guid? excludeCourseId = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Verilen tarih aralığı ve program bilgisine göre uygun derslikleri listeler.
    /// </summary>
    Task<IReadOnlyList<EligibleClassroomResponse>> GetEligibleClassroomsAsync(
        Guid branchId, DateOnly startDate, DateOnly endDate,
        IReadOnlyList<CourseScheduleItemDto> schedules,
        Guid? excludeCourseId = null, CancellationToken cancellationToken = default);
}
