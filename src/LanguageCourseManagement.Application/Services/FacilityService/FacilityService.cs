using AutoMapper;
using FluentValidation;
using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Facilities;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Paging;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace LanguageCourseManagement.Application.Services.FacilityService;

public sealed class FacilityService : IFacilityService
{
    private readonly IFacilityRepository _facilityRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<FacilityService> _logger;
    private readonly IValidator<CreateFacilityRequest> _createValidator;
    private readonly IValidator<UpdateFacilityRequest> _updateValidator;

    public FacilityService(
        IFacilityRepository facilityRepository,
        IMapper mapper,
        ILogger<FacilityService> logger,
        IValidator<CreateFacilityRequest> createValidator,
        IValidator<UpdateFacilityRequest> updateValidator)
    {
        _facilityRepository = facilityRepository;
        _mapper = mapper;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<FacilityResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var facilities = await _facilityRepository.GetAllAsync(cancellationToken);
        return facilities
            .Select(_mapper.Map<FacilityResponse>)
            .ToList();
    }

    public async Task<List<FacilityResponse>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var facilities = await _facilityRepository.GetActiveAsync(cancellationToken);
        return facilities
            .Select(_mapper.Map<FacilityResponse>)
            .ToList();
    }

    public Task<FacilityResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return GetByIdCoreAsync(id, cancellationToken);
    }

    public async Task<GetListResponse<FacilityListResponse>> GetListAsync(
        PageRequest pageRequest,
        string? search,
        bool? isActive,
        bool showDeleted = false,
        CancellationToken cancellationToken = default)
    {
        IPaginate<Facility> facilities = await _facilityRepository.GetListAsync(
            pageRequest.PageIndex,
            pageRequest.PageSize,
            string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            isActive,
            ignoreQueryFilters: showDeleted,
            cancellationToken);

        return new GetListResponse<FacilityListResponse>
        {
            Index = facilities.Index,
            Size = facilities.Size,
            Count = facilities.Count,
            Pages = facilities.Pages,
            HasPrevious = facilities.HasPrevious,
            HasNext = facilities.HasNext,
            Items = facilities.Items.Select(_mapper.Map<FacilityListResponse>).ToList()
        };
    }

    public async Task<FacilityResponse> CreateAsync(
        CreateFacilityRequest request,
        CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_createValidator, request, cancellationToken);
        Normalize(request);
        await EnsureNameIsAvailableAsync(request.Name, cancellationToken: cancellationToken);

        var facility = new Facility
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive
        };

        Facility createdFacility = await _facilityRepository.AddAsync(facility, cancellationToken);
        await _facilityRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("[FacilityService] Facility created - {FacilityId}", createdFacility.Id);
        return _mapper.Map<FacilityResponse>(createdFacility);
    }

    public async Task<FacilityResponse> UpdateAsync(
        Guid id,
        UpdateFacilityRequest request,
        CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_updateValidator, request, cancellationToken);
        Normalize(request);

        var facility = await _facilityRepository.GetByIdAsync(id, cancellationToken);
        if (facility is null)
            throw new NotFoundException("Sosyal olanak bulunamadı.");

        await EnsureNameIsAvailableAsync(
            request.Name,
            id,
            cancellationToken);

        facility.Name = request.Name;
        facility.Description = request.Description;
        facility.IsActive = request.IsActive;

        Facility updatedFacility = await _facilityRepository.UpdateAsync(facility, cancellationToken);
        await _facilityRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("[FacilityService] Facility updated - {FacilityId}", updatedFacility.Id);
        return _mapper.Map<FacilityResponse>(updatedFacility);
    }

    public async Task<FacilityResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var facility = await _facilityRepository.GetByIdAsync(id, cancellationToken);
        if (facility is null)
            throw new NotFoundException("Sosyal olanak bulunamadı.");

        var deletedFacility = await _facilityRepository.DeleteAsync(facility, cancellationToken);
        await _facilityRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("[FacilityService] Facility deleted - {FacilityId}", deletedFacility.Id);
        return _mapper.Map<FacilityResponse>(deletedFacility);
    }

    private async Task<FacilityResponse> GetByIdCoreAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var facility = await _facilityRepository.GetByIdAsync(id, cancellationToken);
        if (facility is null)
            throw new NotFoundException("Sosyal olanak bulunamadı.");

        return _mapper.Map<FacilityResponse>(facility);
    }

    private async Task EnsureNameIsAvailableAsync(
        string name,
        Guid? excludeFacilityId = null,
        CancellationToken cancellationToken = default)
    {
        if (await _facilityRepository.NameExistsAsync(name, excludeFacilityId, cancellationToken))
            throw new BusinessException("Bu isimde bir sosyal olanak zaten mevcut.");
    }

    private static void Normalize(CreateFacilityRequest request)
    {
        request.Name = request.Name.Trim();
        request.Description = NormalizeDescription(request.Description);
    }

    private static void Normalize(UpdateFacilityRequest request)
    {
        request.Name = request.Name.Trim();
        request.Description = NormalizeDescription(request.Description);
    }

    private static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    private static async Task ValidateAsync<TRequest>(
        IValidator<TRequest> validator,
        TRequest request,
        CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(request, cancellationToken);
        if (result.IsValid)
            return;

        throw new LanguageCourseManagement.Application.Exceptions.ValidationException(result.Errors
            .GroupBy(error => error.PropertyName)
            .Select(group => new ValidationExceptionModel
            {
                Property = group.Key,
                Errors = group.Select(error => error.ErrorMessage).ToArray()
            })
            .ToArray());
    }

}
