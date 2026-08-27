using AutoMapper;
using FluentValidation;
using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Schedules;
using LanguageCourseManagement.Application.DTOs.Teachers;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Enums;
using LanguageCourseManagement.Domain.Paging;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace LanguageCourseManagement.Application.Services.TeacherService;

public sealed class TeacherService : ITeacherService
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IOfferedLanguageRepository _offeredLanguageRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TeacherService> _logger;
    private readonly IValidator<CreateTeacherRequest> _createValidator;
    private readonly IValidator<UpdateTeacherRequest> _updateValidator;
    private readonly IValidator<CreateTeacherAvailabilityRequest> _createAvailabilityValidator;
    private readonly IValidator<UpdateTeacherAvailabilityRequest> _updateAvailabilityValidator;

    public TeacherService(
        ITeacherRepository teacherRepository,
        ICourseRepository courseRepository,
        IOfferedLanguageRepository offeredLanguageRepository,
        IBranchRepository branchRepository,
        IMapper mapper,
        ILogger<TeacherService> logger,
        IValidator<CreateTeacherRequest> createValidator,
        IValidator<UpdateTeacherRequest> updateValidator,
        IValidator<CreateTeacherAvailabilityRequest> createAvailabilityValidator,
        IValidator<UpdateTeacherAvailabilityRequest> updateAvailabilityValidator)
    {
        _teacherRepository = teacherRepository;
        _courseRepository = courseRepository;
        _offeredLanguageRepository = offeredLanguageRepository;
        _branchRepository = branchRepository;
        _mapper = mapper;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _createAvailabilityValidator = createAvailabilityValidator;
        _updateAvailabilityValidator = updateAvailabilityValidator;
    }

    public async Task<TeacherResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var teacher = await _teacherRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (teacher is null)
            throw new NotFoundException("Öğretmen bulunamadı.");

        return ToResponse(teacher);
    }

    public async Task<GetListResponse<TeacherListResponse>> GetListAsync(PageRequest pageRequest, string? search, bool? isActive, bool showDeleted = false, CancellationToken cancellationToken = default)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        var searchLower = normalizedSearch?.ToLowerInvariant();
        Expression<Func<Teacher, bool>> predicate = teacher =>
            (!isActive.HasValue || teacher.IsActive == isActive.Value) &&
            (searchLower == null ||
             teacher.FirstName.ToLower().Contains(searchLower) ||
             teacher.LastName.ToLower().Contains(searchLower) ||
             teacher.MobilePhone.ToLower().Contains(searchLower) ||
             (teacher.Email != null && teacher.Email.ToLower().Contains(searchLower)));

        IPaginate<Teacher> teachers;
        if (showDeleted)
        {
            var queryable = _teacherRepository.QueryWithIgnoreFilters().AsNoTracking();
            queryable = queryable.Where(predicate);
            teachers = await queryable
                .OrderBy(teacher => teacher.LastName).ThenBy(teacher => teacher.FirstName)
                .ToPaginateAsync(pageRequest.PageIndex, pageRequest.PageSize, from: 0, cancellationToken);
        }
        else
        {
            teachers = await _teacherRepository.GetListAsync(
                predicate: predicate,
                orderBy: query => query.OrderBy(teacher => teacher.LastName).ThenBy(teacher => teacher.FirstName),
                index: pageRequest.PageIndex,
                size: pageRequest.PageSize,
                enableTracking: false,
                cancellationToken: cancellationToken);
        }

        return new GetListResponse<TeacherListResponse>
        {
            Index = teachers.Index,
            Size = teachers.Size,
            Count = teachers.Count,
            Pages = teachers.Pages,
            HasPrevious = teachers.HasPrevious,
            HasNext = teachers.HasNext,
            Items = teachers.Items.Select(_mapper.Map<TeacherListResponse>).ToList()
        };
    }

    public async Task<TeacherResponse> CreateAsync(CreateTeacherRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_createValidator, request, cancellationToken);
        request.FirstName = request.FirstName.Trim();
        request.LastName = request.LastName.Trim();

        if (request.LanguageIds is null || request.LanguageIds.Count == 0)
            throw new BusinessException("En az bir dil seçilmelidir.");
        if (request.BranchIds is null || request.BranchIds.Count == 0)
            throw new BusinessException("En az bir şube seçilmelidir.");

        var languageIds = await ValidateLanguagesAsync(request.LanguageIds, cancellationToken);
        var branchIds = await ValidateBranchesAsync(request.BranchIds, cancellationToken);

        var teacher = _mapper.Map<Teacher>(request);
        teacher.Id = Guid.NewGuid();
        teacher.IsActive = true;
        teacher.TeacherLanguages = languageIds.Select(languageId => new TeacherLanguage
        {
            Id = Guid.NewGuid(), TeacherId = teacher.Id, OfferedLanguageId = languageId
        }).ToList();
        teacher.TeacherBranches = branchIds.Select(branchId => new TeacherBranch
        {
            Id = Guid.NewGuid(), TeacherId = teacher.Id, BranchId = branchId
        }).ToList();

        await _teacherRepository.AddAsync(teacher, cancellationToken);
        _logger.LogInformation("[TeacherService] Yeni öğretmen oluşturuldu - {TeacherId}", teacher.Id);
        return await GetByIdAsync(teacher.Id, cancellationToken);
    }

    public async Task<TeacherResponse> UpdateAsync(Guid id, UpdateTeacherRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_updateValidator, request, cancellationToken);
        request.FirstName = request.FirstName.Trim();
        request.LastName = request.LastName.Trim();

        var teacher = await _teacherRepository.GetAsync(item => item.Id == id,
            include: query => query.Include(t => t.TeacherLanguages!).Include(t => t.TeacherBranches!),
            cancellationToken: cancellationToken);
        if (teacher is null)
            throw new NotFoundException("Öğretmen bulunamadı.");

        var languageIds = await ValidateLanguagesAsync(request.LanguageIds, cancellationToken);
        var branchIds = await ValidateBranchesAsync(request.BranchIds, cancellationToken);
        teacher.FirstName = request.FirstName;
        teacher.LastName = request.LastName;
        teacher.HomePhone = request.HomePhone;
        teacher.MobilePhone = request.MobilePhone;
        teacher.Email = request.Email;
        teacher.HireDate = request.HireDate;
        teacher.IsActive = request.IsActive;

        teacher.TeacherLanguages?.Clear();
        teacher.TeacherLanguages = languageIds.Select(languageId => new TeacherLanguage
        {
            Id = Guid.NewGuid(), TeacherId = teacher.Id, OfferedLanguageId = languageId
        }).ToList();

        teacher.TeacherBranches?.Clear();
        teacher.TeacherBranches = branchIds.Select(branchId => new TeacherBranch
        {
            Id = Guid.NewGuid(), TeacherId = teacher.Id, BranchId = branchId
        }).ToList();

        await _teacherRepository.UpdateAsync(teacher, cancellationToken);
        _logger.LogInformation("[TeacherService] Öğretmen güncellendi - {TeacherId}", teacher.Id);
        return await GetByIdAsync(teacher.Id, cancellationToken);
    }

    public async Task<TeacherResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var teacher = await _teacherRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (teacher is null)
            throw new NotFoundException("Öğretmen bulunamadı.");

        await _teacherRepository.DeleteAsync(teacher, cancellationToken);
        _logger.LogInformation("[TeacherService] Öğretmen silindi - {TeacherId}", id);
        return ToResponse(teacher);
    }

    public async Task<TeacherAvailabilityResponse> AddAvailabilityAsync(Guid teacherId, CreateTeacherAvailabilityRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_createAvailabilityValidator, request, cancellationToken);
        var teacher = await GetTeacherWithAvailabilitiesAsync(teacherId, cancellationToken);
        if (request.StartTime >= request.EndTime)
            throw new BusinessException("Başlangıç saati bitiş saatinden küçük olmalıdır.");
        if (teacher.Availabilities?.Any(a => a.DayOfWeek == request.DayOfWeek && a.StartTime < request.EndTime && a.EndTime > request.StartTime) == true)
            throw new BusinessException("Bu gün için belirtilen saat aralığında zaten bir müsaitlik kaydı mevcut.");

        var availability = new TeacherAvailability
        {
            Id = Guid.NewGuid(), TeacherId = teacherId, DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime, EndTime = request.EndTime
        };
        (teacher.Availabilities ??= []).Add(availability);
        await _teacherRepository.UpdateAsync(teacher, cancellationToken);
        _logger.LogInformation("[TeacherService] Müsaitlik eklendi - {TeacherId}, {DayOfWeek}", teacherId, request.DayOfWeek);
        return _mapper.Map<TeacherAvailabilityResponse>(availability);
    }

    public async Task<TeacherAvailabilityResponse> UpdateAvailabilityAsync(Guid teacherId, Guid availabilityId, UpdateTeacherAvailabilityRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_updateAvailabilityValidator, request, cancellationToken);
        var teacher = await GetTeacherWithAvailabilitiesAsync(teacherId, cancellationToken);
        if (request.StartTime >= request.EndTime)
            throw new BusinessException("Başlangıç saati bitiş saatinden küçük olmalıdır.");
        var availability = teacher.Availabilities?.FirstOrDefault(a => a.Id == availabilityId);
        if (availability is null)
            throw new NotFoundException("Müsaitlik kaydı bulunamadı.");
        if (teacher.Availabilities?.Any(a => a.Id != availabilityId && a.DayOfWeek == request.DayOfWeek && a.StartTime < request.EndTime && a.EndTime > request.StartTime) == true)
            throw new BusinessException("Bu gün için belirtilen saat aralığında zaten bir müsaitlik kaydı mevcut.");

        availability.DayOfWeek = request.DayOfWeek;
        availability.StartTime = request.StartTime;
        availability.EndTime = request.EndTime;
        await _teacherRepository.UpdateAsync(teacher, cancellationToken);
        _logger.LogInformation("[TeacherService] Müsaitlik güncellendi - {TeacherId}, {AvailabilityId}", teacherId, availabilityId);
        return _mapper.Map<TeacherAvailabilityResponse>(availability);
    }

    public async Task<TeacherAvailabilityResponse> DeleteAvailabilityAsync(Guid teacherId, Guid availabilityId, CancellationToken cancellationToken = default)
    {
        var teacher = await GetTeacherWithAvailabilitiesAsync(teacherId, cancellationToken);
        var availability = teacher.Availabilities?.FirstOrDefault(a => a.Id == availabilityId);
        if (availability is null)
            throw new NotFoundException("Müsaitlik kaydı bulunamadı.");

        teacher.Availabilities!.Remove(availability);
        await _teacherRepository.UpdateAsync(teacher, cancellationToken);
        _logger.LogInformation("[TeacherService] Müsaitlik silindi - {TeacherId}, {AvailabilityId}", teacherId, availabilityId);
        return _mapper.Map<TeacherAvailabilityResponse>(availability);
    }

    public async Task<List<WeeklyScheduleResponse>> GetWeeklyScheduleAsync(Guid teacherId, CancellationToken cancellationToken)
    {
        var teacher = await _teacherRepository.GetByIdWithDetailsAsync(teacherId, cancellationToken);
        if (teacher is null)
            throw new NotFoundException("Öğretmen bulunamadı.");

        var courses = await _courseRepository.Query()
            .Where(c => c.TeacherId == teacherId && c.IsActive)
            .Select(c => new
            {
                c.Name,
                BranchName = c.Branch.Name,
                c.Schedules,
                ActiveStudentCount = c.Enrollments!.Count(e => e.Status == EnrollmentStatus.Active)
            })
            .ToListAsync(cancellationToken);

        var result = courses
            .SelectMany(c => c.Schedules ?? [], (c, s) => new WeeklyScheduleResponse
            {
                CourseName = c.Name,
                BranchName = c.BranchName,
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                StudentCount = c.ActiveStudentCount
            })
            .OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartTime)
            .ToList();

        return result;
    }

    private async Task<Teacher> GetTeacherWithAvailabilitiesAsync(Guid teacherId, CancellationToken cancellationToken)
    {
        var teacher = await _teacherRepository.GetAsync(t => t.Id == teacherId,
            include: query => query.Include(t => t.Availabilities!), cancellationToken: cancellationToken);
        return teacher ?? throw new NotFoundException("Öğretmen bulunamadı.");
    }

    private async Task<List<Guid>> ValidateLanguagesAsync(List<Guid> languageIds, CancellationToken cancellationToken)
    {
        if (languageIds is null || languageIds.Count == 0)
            return [];
        var distinctIds = languageIds.Distinct().ToList();
        var result = await _offeredLanguageRepository.GetListAsync(
            predicate: l => distinctIds.Contains(l.Id) && l.IsActive,
            index: 0,
            size: distinctIds.Count,
            enableTracking: false,
            cancellationToken: cancellationToken);
        if (result.Count != distinctIds.Count)
            throw new BusinessException("Seçilen dillerden biri veya daha fazlası bulunamadı ya da pasif durumda.");
        return distinctIds;
    }

    private async Task<List<Guid>> ValidateBranchesAsync(List<Guid> branchIds, CancellationToken cancellationToken)
    {
        if (branchIds is null || branchIds.Count == 0)
            return [];
        var distinctIds = branchIds.Distinct().ToList();
        var result = await _branchRepository.GetListAsync(
            predicate: b => distinctIds.Contains(b.Id) && b.IsActive,
            index: 0,
            size: distinctIds.Count,
            enableTracking: false,
            cancellationToken: cancellationToken);
        if (result.Count != distinctIds.Count)
            throw new BusinessException("Seçilen şubelerden biri veya daha fazlası bulunamadı ya da pasif durumda.");
        return distinctIds;
    }

    private TeacherResponse ToResponse(Teacher teacher)
    {
        var response = _mapper.Map<TeacherResponse>(teacher);
        response.LanguageIds = teacher.TeacherLanguages?.Select(tl => tl.OfferedLanguageId).ToList() ?? [];
        response.BranchIds = teacher.TeacherBranches?.Select(tb => tb.BranchId).ToList() ?? [];
        response.Availabilities = teacher.Availabilities?.Select(a => _mapper.Map<TeacherAvailabilityResponse>(a)).ToList() ?? [];
        return response;
    }

    private static async Task ValidateAsync<TRequest>(IValidator<TRequest> validator, TRequest request, CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(request, cancellationToken);
        if (result.IsValid)
            return;
        throw new LanguageCourseManagement.Application.Exceptions.ValidationException(result.Errors
            .GroupBy(error => error.PropertyName)
            .Select(group => new LanguageCourseManagement.Application.Exceptions.ValidationExceptionModel
            {
                Property = group.Key, Errors = group.Select(error => error.ErrorMessage).ToArray()
            }).ToArray());
    }
}
