using AutoMapper;
using FluentValidation;
using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.CourseLevels;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Paging;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace LanguageCourseManagement.Application.Services.CourseLevelService;

public sealed class CourseLevelService : ICourseLevelService
{
    private readonly ICourseLevelRepository _courseLevelRepository;
    private readonly IOfferedLanguageRepository _offeredLanguageRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CourseLevelService> _logger;
    private readonly IValidator<CreateCourseLevelRequest> _createValidator;
    private readonly IValidator<UpdateCourseLevelRequest> _updateValidator;

    public CourseLevelService(ICourseLevelRepository courseLevelRepository, IOfferedLanguageRepository offeredLanguageRepository, IMapper mapper, ILogger<CourseLevelService> logger, IValidator<CreateCourseLevelRequest> createValidator, IValidator<UpdateCourseLevelRequest> updateValidator)
    {
        _courseLevelRepository = courseLevelRepository;
        _offeredLanguageRepository = offeredLanguageRepository;
        _mapper = mapper;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<CourseLevelResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var level = await _courseLevelRepository.GetAsync(item => item.Id == id, include: query => query.Include(item => item.OfferedLanguage), enableTracking: false, cancellationToken: cancellationToken);
        if (level is null)
            throw new NotFoundException("Kurs seviyesi bulunamadı.");
        return _mapper.Map<CourseLevelResponse>(level);
    }

    public async Task<GetListResponse<CourseLevelListResponse>> GetListAsync(PageRequest pageRequest, string? search, Guid? offeredLanguageId, bool? isActive, CancellationToken cancellationToken = default)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        Expression<Func<CourseLevel, bool>> predicate = level =>
            (!offeredLanguageId.HasValue || level.OfferedLanguageId == offeredLanguageId.Value) &&
            (!isActive.HasValue || level.IsActive == isActive.Value) &&
            (normalizedSearch == null || level.Name.Contains(normalizedSearch) ||
             (level.Description != null && level.Description.Contains(normalizedSearch)) ||
             (level.OfferedLanguage != null && level.OfferedLanguage.Name.Contains(normalizedSearch)));

        IPaginate<CourseLevel> levels = await _courseLevelRepository.GetListAsync(predicate: predicate, orderBy: query => query.OrderBy(level => level.OfferedLanguage.Name).ThenBy(level => level.Order).ThenBy(level => level.Name), include: query => query.Include(level => level.OfferedLanguage), index: pageRequest.PageIndex, size: pageRequest.PageSize, enableTracking: false, cancellationToken: cancellationToken);
        return new GetListResponse<CourseLevelListResponse>
        {
            Index = levels.Index, Size = levels.Size, Count = levels.Count, Pages = levels.Pages,
            HasPrevious = levels.HasPrevious, HasNext = levels.HasNext,
            Items = levels.Items.Select(_mapper.Map<CourseLevelListResponse>).ToList()
        };
    }

    public async Task<CourseLevelResponse> CreateAsync(CreateCourseLevelRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_createValidator, request, cancellationToken);
        request.Name = request.Name.Trim();
        request.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        await EnsureLanguageExistsAsync(request.OfferedLanguageId, requireActive: true, cancellationToken);
        if (await _courseLevelRepository.NameExistsAsync(request.OfferedLanguageId, request.Name, cancellationToken: cancellationToken))
            throw new BusinessException("Bu dilde aynı isimde bir kurs seviyesi zaten mevcut.");
        var level = _mapper.Map<CourseLevel>(request);
        level.Id = Guid.NewGuid();
        level.IsActive = true;
        await _courseLevelRepository.AddAsync(level, cancellationToken);
        _logger.LogInformation("[CourseLevelService] Yeni kurs seviyesi oluşturuldu - {CourseLevelId}", level.Id);
        return await GetByIdAsync(level.Id, cancellationToken);
    }

    public async Task<CourseLevelResponse> UpdateAsync(Guid id, UpdateCourseLevelRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_updateValidator, request, cancellationToken);
        request.Name = request.Name.Trim();
        request.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        var level = await _courseLevelRepository.GetAsync(item => item.Id == id, cancellationToken: cancellationToken);
        if (level is null)
            throw new NotFoundException("Kurs seviyesi bulunamadı.");
        await EnsureLanguageExistsAsync(request.OfferedLanguageId, requireActive: request.OfferedLanguageId != level.OfferedLanguageId, cancellationToken);
        if (await _courseLevelRepository.NameExistsAsync(request.OfferedLanguageId, request.Name, excludeCourseLevelId: id, cancellationToken: cancellationToken))
            throw new BusinessException("Bu dilde aynı isimde bir kurs seviyesi zaten mevcut.");
        level.OfferedLanguageId = request.OfferedLanguageId;
        level.Name = request.Name;
        level.Description = request.Description;
        level.Order = request.Order;
        level.IsActive = request.IsActive;
        await _courseLevelRepository.UpdateAsync(level, cancellationToken);
        _logger.LogInformation("[CourseLevelService] Kurs seviyesi güncellendi - {CourseLevelId}", level.Id);
        return await GetByIdAsync(level.Id, cancellationToken);
    }

    public async Task<CourseLevelResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var level = await _courseLevelRepository.GetAsync(item => item.Id == id, include: query => query.Include(item => item.OfferedLanguage), cancellationToken: cancellationToken);
        if (level is null)
            throw new NotFoundException("Kurs seviyesi bulunamadı.");
        var response = _mapper.Map<CourseLevelResponse>(level);
        await _courseLevelRepository.DeleteAsync(level, cancellationToken);
        _logger.LogInformation("[CourseLevelService] Kurs seviyesi silindi - {CourseLevelId}", level.Id);
        return response;
    }

    private async Task EnsureLanguageExistsAsync(Guid offeredLanguageId, bool requireActive, CancellationToken cancellationToken)
    {
        var language = await _offeredLanguageRepository.GetAsync(item => item.Id == offeredLanguageId, enableTracking: false, cancellationToken: cancellationToken);
        if (language is null)
            throw new BusinessException("Seçilen dil bulunamadı.");
        if (requireActive && !language.IsActive)
            throw new BusinessException("Kurs seviyesi yalnızca aktif bir dile eklenebilir.");
    }

    private static async Task ValidateAsync<TRequest>(IValidator<TRequest> validator, TRequest request, CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(request, cancellationToken);
        if (result.IsValid)
            return;
        throw new LanguageCourseManagement.Application.Exceptions.ValidationException(result.Errors
            .GroupBy(error => error.PropertyName)
            .Select(group => new LanguageCourseManagement.Application.Exceptions.ValidationExceptionModel { Property = group.Key, Errors = group.Select(error => error.ErrorMessage).ToArray() })
            .ToArray());
    }
}
