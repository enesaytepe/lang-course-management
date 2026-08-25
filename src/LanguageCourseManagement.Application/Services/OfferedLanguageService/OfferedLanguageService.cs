using AutoMapper;
using FluentValidation;
using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.OfferedLanguages;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Paging;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace LanguageCourseManagement.Application.Services.OfferedLanguageService;

public sealed class OfferedLanguageService : IOfferedLanguageService
{
    private readonly IOfferedLanguageRepository _offeredLanguageRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<OfferedLanguageService> _logger;
    private readonly IValidator<CreateOfferedLanguageRequest> _createValidator;
    private readonly IValidator<UpdateOfferedLanguageRequest> _updateValidator;

    public OfferedLanguageService(IOfferedLanguageRepository offeredLanguageRepository, IMapper mapper, ILogger<OfferedLanguageService> logger, IValidator<CreateOfferedLanguageRequest> createValidator, IValidator<UpdateOfferedLanguageRequest> updateValidator)
    {
        _offeredLanguageRepository = offeredLanguageRepository;
        _mapper = mapper;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<OfferedLanguageResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var language = await _offeredLanguageRepository.GetAsync(item => item.Id == id, enableTracking: false, cancellationToken: cancellationToken);
        if (language is null)
            throw new NotFoundException("Dil bulunamadı.");
        return _mapper.Map<OfferedLanguageResponse>(language);
    }

    public async Task<GetListResponse<OfferedLanguageListResponse>> GetListAsync(PageRequest pageRequest, string? search, bool? isActive, CancellationToken cancellationToken = default)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        Expression<Func<OfferedLanguage, bool>> predicate = language =>
            (!isActive.HasValue || language.IsActive == isActive.Value) &&
            (normalizedSearch == null || language.Name.Contains(normalizedSearch) || (language.Code != null && language.Code.Contains(normalizedSearch)));

        IPaginate<OfferedLanguage> languages = await _offeredLanguageRepository.GetListAsync(predicate: predicate, orderBy: query => query.OrderBy(language => language.Name), index: pageRequest.PageIndex, size: pageRequest.PageSize, enableTracking: false, cancellationToken: cancellationToken);
        return new GetListResponse<OfferedLanguageListResponse>
        {
            Index = languages.Index, Size = languages.Size, Count = languages.Count, Pages = languages.Pages,
            HasPrevious = languages.HasPrevious, HasNext = languages.HasNext,
            Items = languages.Items.Select(_mapper.Map<OfferedLanguageListResponse>).ToList()
        };
    }

    public async Task<OfferedLanguageResponse> CreateAsync(CreateOfferedLanguageRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_createValidator, request, cancellationToken);
        request.Name = request.Name.Trim();
        request.Code = string.IsNullOrWhiteSpace(request.Code) ? null : request.Code.Trim();
        if (await _offeredLanguageRepository.NameExistsAsync(request.Name, cancellationToken: cancellationToken))
            throw new BusinessException("Bu isimde bir dil zaten mevcut.");
        var language = _mapper.Map<OfferedLanguage>(request);
        language.Id = Guid.NewGuid();
        language.IsActive = true;
        await _offeredLanguageRepository.AddAsync(language, cancellationToken);
        _logger.LogInformation("[OfferedLanguageService] Yeni dil oluşturuldu - {OfferedLanguageId}", language.Id);
        return await GetByIdAsync(language.Id, cancellationToken);
    }

    public async Task<OfferedLanguageResponse> UpdateAsync(Guid id, UpdateOfferedLanguageRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_updateValidator, request, cancellationToken);
        request.Name = request.Name.Trim();
        request.Code = string.IsNullOrWhiteSpace(request.Code) ? null : request.Code.Trim();
        var language = await _offeredLanguageRepository.GetAsync(item => item.Id == id, cancellationToken: cancellationToken);
        if (language is null)
            throw new NotFoundException("Dil bulunamadı.");
        if (await _offeredLanguageRepository.NameExistsAsync(request.Name, excludeOfferedLanguageId: id, cancellationToken: cancellationToken))
            throw new BusinessException("Bu isimde bir dil zaten mevcut.");
        language.Name = request.Name;
        language.Code = request.Code;
        language.IsActive = request.IsActive;
        await _offeredLanguageRepository.UpdateAsync(language, cancellationToken);
        _logger.LogInformation("[OfferedLanguageService] Dil güncellendi - {OfferedLanguageId}", language.Id);
        return await GetByIdAsync(language.Id, cancellationToken);
    }

    public async Task<OfferedLanguageResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var language = await _offeredLanguageRepository.GetAsync(item => item.Id == id, cancellationToken: cancellationToken);
        if (language is null)
            throw new NotFoundException("Dil bulunamadı.");
        var response = _mapper.Map<OfferedLanguageResponse>(language);
        await _offeredLanguageRepository.DeleteAsync(language, cancellationToken);
        _logger.LogInformation("[OfferedLanguageService] Dil silindi - {OfferedLanguageId}", language.Id);
        return response;
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
