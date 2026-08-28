using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Classrooms;
using LanguageCourseManagement.Application.DTOs.Schedules;

namespace LanguageCourseManagement.Application.Services.ClassroomService;

/// <summary>
/// Derslik işlemlerini tanımlar.
/// </summary>
public interface IClassroomService
{
    /// <summary>
    /// ID'ye göre dersliği getirir.
    /// </summary>
    Task<ClassroomResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Arama ve filtre kriterlerine göre derslikleri sayfalamalı getirir.
    /// </summary>
    Task<GetListResponse<ClassroomListResponse>> GetListAsync(
        PageRequest pageRequest,
        string? search,
        Guid? branchId,
        bool? isActive,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Yeni derslik oluşturur.
    /// </summary>
    Task<ClassroomResponse> CreateAsync(
        CreateClassroomRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mevcut dersliği günceller.
    /// </summary>
    Task<ClassroomResponse> UpdateAsync(
        Guid id,
        UpdateClassroomRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Dersliği soft delete ile siler.
    /// </summary>
    Task<ClassroomResponse> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Dersliğin haftalık ders programını getirir.
    /// </summary>
    Task<List<WeeklyScheduleResponse>> GetWeeklyScheduleAsync(
        Guid classroomId,
        CancellationToken cancellationToken = default);
}