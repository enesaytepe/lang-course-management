using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Schedules;
using LanguageCourseManagement.Application.DTOs.Teachers;

namespace LanguageCourseManagement.Application.Services.TeacherService;

/// <summary>
/// Öğretmen işlemlerini tanımlar.
/// </summary>
public interface ITeacherService
{
    /// <summary>
    /// ID'ye göre öğretmeni getirir.
    /// </summary>
    Task<TeacherResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Arama ve filtre kriterlerine göre öğretmenleri sayfalamalı getirir.
    /// </summary>
    Task<GetListResponse<TeacherListResponse>> GetListAsync(PageRequest pageRequest, string? search, bool? isActive, CancellationToken cancellationToken = default);
    /// <summary>
    /// Yeni öğretmen oluşturur.
    /// </summary>
    Task<TeacherResponse> CreateAsync(CreateTeacherRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Mevcut öğretmeni günceller.
    /// </summary>
    Task<TeacherResponse> UpdateAsync(Guid id, UpdateTeacherRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Öğretmeni soft delete ile siler.
    /// </summary>
    Task<TeacherResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Öğretmen için yeni müsaitlik kaydı ekler.
    /// </summary>
    Task<TeacherAvailabilityResponse> AddAvailabilityAsync(Guid teacherId, CreateTeacherAvailabilityRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Mevcut müsaitlik kaydını günceller.
    /// </summary>
    Task<TeacherAvailabilityResponse> UpdateAvailabilityAsync(Guid teacherId, Guid availabilityId, UpdateTeacherAvailabilityRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Müsaitlik kaydını siler.
    /// </summary>
    Task<TeacherAvailabilityResponse> DeleteAvailabilityAsync(Guid teacherId, Guid availabilityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Öğretmenin haftalık ders programını getirir.
    /// </summary>
    Task<List<WeeklyScheduleResponse>> GetWeeklyScheduleAsync(Guid teacherId, CancellationToken cancellationToken = default);
}
