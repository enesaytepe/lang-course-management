using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Classrooms;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Paging;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace LanguageCourseManagement.Application.Services.ClassroomService;

public sealed class ClassroomService : IClassroomService
{
    private readonly IClassroomRepository _classroomRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ClassroomService> _logger;
    private readonly IValidator<CreateClassroomRequest> _createValidator;
    private readonly IValidator<UpdateClassroomRequest> _updateValidator;

    public ClassroomService(
        IClassroomRepository classroomRepository,
        IBranchRepository branchRepository,
        IMapper mapper,
        ILogger<ClassroomService> logger,
        IValidator<CreateClassroomRequest> createValidator,
        IValidator<UpdateClassroomRequest> updateValidator)
    {
        _classroomRepository = classroomRepository;
        _branchRepository = branchRepository;
        _mapper = mapper;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ClassroomResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var classroom = await _classroomRepository.Query()
            .Where(item => item.Id == id)
            .ProjectTo<ClassroomResponse>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (classroom is null)
            throw new NotFoundException("Derslik bulunamadı.");

        return classroom;
    }

    public async Task<GetListResponse<ClassroomListResponse>> GetListAsync(
        PageRequest pageRequest,
        string? search,
        Guid? branchId,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        Expression<Func<Classroom, bool>> predicate = classroom =>
            (!branchId.HasValue || classroom.BranchId == branchId.Value) &&
            (!isActive.HasValue || classroom.IsActive == isActive.Value) &&
            (normalizedSearch == null ||
             classroom.Name.Contains(normalizedSearch) ||
             (classroom.Description != null && classroom.Description.Contains(normalizedSearch)) ||
             classroom.Branch.Name.Contains(normalizedSearch));

        var classrooms = await _classroomRepository.Query()
            .Where(predicate)
            .OrderBy(classroom => classroom.Branch.Name).ThenBy(classroom => classroom.Name)
            .ProjectTo<ClassroomListResponse>(_mapper.ConfigurationProvider)
            .ToPaginateAsync(pageRequest.PageIndex, pageRequest.PageSize, cancellationToken: cancellationToken);

        return new GetListResponse<ClassroomListResponse>
        {
            Index = classrooms.Index,
            Size = classrooms.Size,
            Count = classrooms.Count,
            Pages = classrooms.Pages,
            HasPrevious = classrooms.HasPrevious,
            HasNext = classrooms.HasNext,
            Items = classrooms.Items.ToList()
        };
    }

    public async Task<ClassroomResponse> CreateAsync(
        CreateClassroomRequest request,
        CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_createValidator, request, cancellationToken);

        request.Name = request.Name.Trim();
        request.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        await EnsureBranchExistsAsync(request.BranchId, requireActive: true, cancellationToken);

        if (await _classroomRepository.NameExistsAsync(
            request.BranchId,
            request.Name,
            cancellationToken: cancellationToken))
        {
            throw new BusinessException("Bu şubede aynı isimde bir derslik zaten mevcut.");
        }

        var classroom = _mapper.Map<Classroom>(request);
        classroom.Id = Guid.NewGuid();
        classroom.IsActive = true;

        await _classroomRepository.AddAsync(classroom);

        _logger.LogInformation(
            "[ClassroomService] Yeni derslik oluşturuldu - {ClassroomId}",
            classroom.Id);

        return await GetByIdAsync(classroom.Id, cancellationToken);
    }

    public async Task<ClassroomResponse> UpdateAsync(
        Guid id,
        UpdateClassroomRequest request,
        CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_updateValidator, request, cancellationToken);

        request.Name = request.Name.Trim();
        request.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        var classroom = await _classroomRepository.GetAsync(
            item => item.Id == id,
            cancellationToken: cancellationToken);

        if (classroom is null)
            throw new NotFoundException("Derslik bulunamadı.");

        await EnsureBranchExistsAsync(
            request.BranchId,
            requireActive: request.BranchId != classroom.BranchId,
            cancellationToken);

        if (await _classroomRepository.NameExistsAsync(
            request.BranchId,
            request.Name,
            excludeClassroomId: id,
            cancellationToken: cancellationToken))
        {
            throw new BusinessException("Bu şubede aynı isimde bir derslik zaten mevcut.");
        }

        classroom.BranchId = request.BranchId;
        classroom.Name = request.Name;
        classroom.Description = request.Description;
        classroom.Capacity = request.Capacity;
        classroom.IsActive = request.IsActive;

        await _classroomRepository.UpdateAsync(classroom);

        _logger.LogInformation(
            "[ClassroomService] Derslik güncellendi - {ClassroomId}",
            classroom.Id);

        return await GetByIdAsync(classroom.Id, cancellationToken);
    }

    public async Task<ClassroomResponse> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var classroom = await _classroomRepository.GetAsync(
            item => item.Id == id,
            cancellationToken: cancellationToken);

        if (classroom is null)
            throw new NotFoundException("Derslik bulunamadı.");

        var response = await _classroomRepository.Query()
            .Where(item => item.Id == id)
            .ProjectTo<ClassroomResponse>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Derslik bulunamadı.");

        await _classroomRepository.DeleteAsync(classroom);

        _logger.LogInformation(
            "[ClassroomService] Derslik silindi - {ClassroomId}",
            classroom.Id);

        return response;
    }

    private async Task EnsureBranchExistsAsync(
        Guid branchId,
        bool requireActive,
        CancellationToken cancellationToken)
    {
        var branch = await _branchRepository.GetAsync(
            item => item.Id == branchId,
            enableTracking: false,
            cancellationToken: cancellationToken);

        if (branch is null)
            throw new BusinessException("Seçilen şube bulunamadı.");

        if (requireActive && !branch.IsActive)
            throw new BusinessException("Yeni derslik yalnızca aktif bir şubeye bağlanabilir.");
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
            .Select(group => new LanguageCourseManagement.Application.Exceptions.ValidationExceptionModel
            {
                Property = group.Key,
                Errors = group.Select(error => error.ErrorMessage).ToArray()
            })
            .ToArray());
    }
}
