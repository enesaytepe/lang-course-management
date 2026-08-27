using AutoMapper;
using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Branches;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Paging;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Linq.Expressions;

namespace LanguageCourseManagement.Application.Services.BranchService;

/// <inheritdoc />
public class BranchService : IBranchService
{
    private readonly IBranchRepository _branchRepository;
    private readonly IMapper _mapper;
    private readonly IFacilityRepository _facilityRepository;
    private readonly IClassroomRepository _classroomRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IOfferedLanguageRepository _offeredLanguageRepository;
    private readonly ILogger<BranchService> _logger;

    public BranchService(
        IBranchRepository branchRepository,
        IMapper mapper,
        IFacilityRepository facilityRepository,
        IClassroomRepository classroomRepository,
        ICourseRepository courseRepository,
        ITeacherRepository teacherRepository,
        IOfferedLanguageRepository offeredLanguageRepository,
        ILogger<BranchService> logger)
    {
        _branchRepository = branchRepository;
        _mapper = mapper;
        _facilityRepository = facilityRepository;
        _classroomRepository = classroomRepository;
        _courseRepository = courseRepository;
        _teacherRepository = teacherRepository;
        _offeredLanguageRepository = offeredLanguageRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<BranchResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Branch? branch = await _branchRepository.GetAsync(
            b => b.Id == id,
            include: query => query.Include(b => b.BranchFacilities!),
            cancellationToken: cancellationToken);

        if (branch is null)
            throw new NotFoundException("Şube bulunamadı.");

        _logger.LogInformation("[BranchService] Sube detay getirildi - {BranchId}", id);
        return ToResponse(branch);
    }

    /// <inheritdoc />
    public async Task<GetListResponse<BranchListResponse>> GetListAsync(
        PageRequest pageRequest,
        string? search,
        bool? isActive,
        bool showDeleted = false,
        CancellationToken cancellationToken = default)
    {
        // Arama ve şube durumuna göre dinamik filtre oluştur
        Expression<Func<Branch, bool>>? predicate = null;

        if (!string.IsNullOrWhiteSpace(search) && isActive.HasValue)
        {
            var searchLower = search.ToLowerInvariant();
            predicate = b =>
                (b.Name.ToLower().Contains(searchLower) ||
                 b.Address.ToLower().Contains(searchLower)) &&
                 b.IsActive == isActive.Value;
        }
        else if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLowerInvariant();
            predicate = b =>
                b.Name.ToLower().Contains(searchLower) ||
                b.Address.ToLower().Contains(searchLower);
        }
        else if (isActive.HasValue)
        {
            predicate = b => b.IsActive == isActive.Value;
        }

        IPaginate<Branch> branches;
        if (showDeleted)
        {
            var queryable = _branchRepository.QueryWithIgnoreFilters().AsNoTracking();
            if (predicate != null)
                queryable = queryable.Where(predicate);
            branches = await queryable
                .OrderByDescending(b => b.CreatedAt)
                .ToPaginateAsync(pageRequest.PageIndex, pageRequest.PageSize, from: 0, cancellationToken);
        }
        else
        {
            branches = await _branchRepository.GetListAsync(
                predicate: predicate,
                orderBy: q => q.OrderByDescending(b => b.CreatedAt),
                index: pageRequest.PageIndex,
                size: pageRequest.PageSize,
                cancellationToken: cancellationToken);
        }

        _logger.LogInformation("[BranchService] Sube listesi getirildi - Sayfa: {PageIndex}, Boyut: {PageSize}", pageRequest.PageIndex, pageRequest.PageSize);
        return new GetListResponse<BranchListResponse>
        {
            Index = branches.Index,
            Size = branches.Size,
            Count = branches.Count,
            Pages = branches.Pages,
            HasPrevious = branches.HasPrevious,
            HasNext = branches.HasNext,
            Items = branches.Items.Select(_mapper.Map<BranchListResponse>).ToList()
        };
    }

    /// <inheritdoc />
    public async Task<BranchResponse> CreateAsync(CreateBranchRequest request, CancellationToken cancellationToken = default)
    {
        bool exists = await _branchRepository.NameExistsAsync(request.Name!);
        if (exists)
            throw new BusinessException("Bu isimde bir şube zaten mevcut.");

        var facilityIds = await ValidateFacilitiesAsync(request.FacilityIds?.ToList() ?? [], requireActive: true, cancellationToken);
        Branch branch = _mapper.Map<Branch>(request);
        branch.Id = Guid.NewGuid();
        branch.IsActive = true;
        branch.BranchFacilities = facilityIds
            .Select(facilityId => new BranchFacility
            {
                Id = Guid.NewGuid(),
                BranchId = branch.Id,
                FacilityId = facilityId
            })
            .ToList();

        Branch createdBranch = await _branchRepository.AddAsync(branch);
        _logger.LogInformation("[BranchService] Yeni sube olusturuldu - {BranchId}", createdBranch.Id);
        return ToResponse(createdBranch);
    }

    /// <inheritdoc />
    public async Task<BranchResponse> UpdateAsync(Guid id, UpdateBranchRequest request, CancellationToken cancellationToken = default)
    {
        Branch? branch = await _branchRepository.GetAsync(
            b => b.Id == id,
            include: query => query.Include(b => b.BranchFacilities!),
            cancellationToken: cancellationToken);

        if (branch is null)
            throw new NotFoundException("Şube bulunamadı.");

        // excludeBranchId: Güncelleme sırasında şube mevcut ismini koruyabilmeli; kendi kaydı dışlanır
        bool exists = await _branchRepository.NameExistsAsync(request.Name, excludeBranchId: branch.Id);
        if (exists)
            throw new BusinessException("Bu isimde bir şube zaten mevcut.");

        var requestedFacilityIds = request.FacilityIds ?? [];
        var currentFacilityIds = branch.BranchFacilities?.Select(link => link.FacilityId).ToList() ?? [];
        var activeCurrentFacilityIds = await _facilityRepository.GetActiveIdsAsync(currentFacilityIds, cancellationToken);
        var inactiveCurrentFacilityIds = currentFacilityIds.Except(activeCurrentFacilityIds);
        var newlyRequestedFacilityIds = requestedFacilityIds.Except(currentFacilityIds).ToList();

        // New assignments must be active. Existing inactive assignments are retained below
        // for backwards compatibility, while the final existence check rejects deleted IDs.
        await ValidateFacilitiesAsync(newlyRequestedFacilityIds, requireActive: true, cancellationToken);

        var facilityIdsToPersist = requestedFacilityIds
            .Concat(inactiveCurrentFacilityIds)
            .Distinct()
            .ToList();
        var facilityIds = await ValidateFacilitiesAsync(facilityIdsToPersist, requireActive: false, cancellationToken);
        branch!.Name = request.Name;
        branch.Address = request.Address;
        branch.PublicTransportationDirections = request.PublicTransportationDirections;
        branch.PrivateVehicleDirections = request.PrivateVehicleDirections;
        branch.Latitude = request.Latitude;
        branch.Longitude = request.Longitude;
        branch.PhoneNumber = request.PhoneNumber;
        branch.IsActive = request.IsActive;

        Branch updatedBranch = await _branchRepository.UpdateWithFacilitiesAsync(branch, facilityIds, cancellationToken);
        _logger.LogInformation("[BranchService] Sube guncellendi - {BranchId}", id);
        return ToResponse(updatedBranch);
    }

    /// <inheritdoc />
    public async Task<BranchResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Branch? branch = await _branchRepository.GetAsync(b => b.Id == id, cancellationToken: cancellationToken);
        if (branch is null)
            throw new NotFoundException("Şube bulunamadı.");

        Branch deletedBranch = await _branchRepository.DeleteAsync(branch!);
        _logger.LogInformation("[BranchService] Sube silindi - {BranchId}", id);

        return ToResponse(deletedBranch);
    }

    private async Task<List<Guid>> ValidateFacilitiesAsync(
        List<Guid> facilityIds,
        bool requireActive,
        CancellationToken cancellationToken)
    {
        var distinctIds = facilityIds.Distinct().ToList();
        var validIds = requireActive
            ? await _facilityRepository.GetActiveIdsAsync(distinctIds, cancellationToken)
            : await _facilityRepository.GetIdsAsync(distinctIds, cancellationToken);
        if (validIds.Count != distinctIds.Count)
            throw new BusinessException("Seçilen sosyal olanaklardan biri veya daha fazlası bulunamadı ya da pasif durumda.");

        return distinctIds;
    }

    /// <inheritdoc />
    public async Task<BranchDetailsResponse> GetDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Branch? branch = await _branchRepository.GetAsync(
            b => b.Id == id,
            include: query => query.Include(b => b.BranchFacilities!),
            cancellationToken: cancellationToken);

        if (branch is null)
            throw new NotFoundException("Şube bulunamadı.");

        var facilityIds = branch.BranchFacilities?.Select(link => link.FacilityId).ToList() ?? [];
        var facilityNames = new List<string>();
        if (facilityIds.Count > 0)
        {
            var allFacilities = await _facilityRepository.GetAllAsync(cancellationToken);
            facilityNames = allFacilities
                .Where(f => facilityIds.Contains(f.Id))
                .Select(f => f.Name)
                .ToList();
        }

        // Derslikleri getir (şubeye ait)
        var classroomItems = await _classroomRepository.Query()
            .AsNoTracking()
            .Where(c => c.BranchId == id && !c.IsDeleted)
            .OrderBy(c => c.Name)
            .Select(c => new BranchClassroomItem
            {
                Id = c.Id,
                Name = c.Name,
                Capacity = c.Capacity,
                IsActive = c.IsActive
            })
            .ToListAsync(cancellationToken);

        // Kursları getir (şubeye ait)
        var courseItems = await _courseRepository.Query()
            .AsNoTracking()
            .Where(c => c.BranchId == id && !c.IsDeleted)
            .OrderByDescending(c => c.StartDate)
            .Select(c => new BranchCourseItem
            {
                Id = c.Id,
                Name = c.Name,
                LevelName = c.CourseLevel.Name,
                TeacherName = c.Teacher.FirstName + " " + c.Teacher.LastName,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = c.Status,
                IsActive = c.IsActive
            })
            .ToListAsync(cancellationToken);

        // Öğretmenleri getir (şubede ders verebilen)
        var teacherIds = await _teacherRepository.Query()
            .AsNoTracking()
            .Where(t => !t.IsDeleted && t.TeacherBranches != null && t.TeacherBranches.Any(tb => tb.BranchId == id))
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        var teacherItems = new List<BranchTeacherItem>();
        if (teacherIds.Count > 0)
        {
            teacherItems = await _teacherRepository.Query()
                .AsNoTracking()
                .Where(t => teacherIds.Contains(t.Id) && !t.IsDeleted)
                .OrderBy(t => t.LastName).ThenBy(t => t.FirstName)
                .Select(t => new BranchTeacherItem
                {
                    Id = t.Id,
                    FirstName = t.FirstName,
                    LastName = t.LastName,
                    MobilePhone = t.MobilePhone,
                    IsActive = t.IsActive,
                    Languages = t.TeacherLanguages != null
                        ? t.TeacherLanguages.Select(tl => tl.OfferedLanguage.Name).ToList()
                        : new List<string>()
                })
                .ToListAsync(cancellationToken);
        }

        _logger.LogInformation("[BranchService] Sube detay bilgileri getirildi - {BranchId}", id);

        return new BranchDetailsResponse
        {
            Id = branch.Id,
            Name = branch.Name ?? string.Empty,
            Address = branch.Address ?? string.Empty,
            PublicTransportationDirections = branch.PublicTransportationDirections,
            PrivateVehicleDirections = branch.PrivateVehicleDirections,
            PhoneNumber = branch.PhoneNumber,
            Latitude = branch.Latitude,
            Longitude = branch.Longitude,
            IsActive = branch.IsActive,
            FacilityNames = facilityNames,
            Classrooms = classroomItems,
            Courses = courseItems,
            Teachers = teacherItems
        };
    }

    private BranchResponse ToResponse(Branch branch)
    {
        var response = _mapper.Map<BranchResponse>(branch);
        response.FacilityIds = branch.BranchFacilities?
            .Select(link => link.FacilityId)
            .ToArray() ?? [];
        return response;
    }
}
