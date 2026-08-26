using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Facilities;

namespace LanguageCourseManagement.Application.Services.FacilityService;

public interface IFacilityService
{
    Task<List<FacilityResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<FacilityResponse>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<FacilityResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetListResponse<FacilityListResponse>> GetListAsync(
        PageRequest pageRequest,
        string? search,
        bool? isActive,
        bool showDeleted = false,
        CancellationToken cancellationToken = default);
    Task<FacilityResponse> CreateAsync(
        CreateFacilityRequest request,
        CancellationToken cancellationToken = default);
    Task<FacilityResponse> UpdateAsync(
        Guid id,
        UpdateFacilityRequest request,
        CancellationToken cancellationToken = default);
    Task<FacilityResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
