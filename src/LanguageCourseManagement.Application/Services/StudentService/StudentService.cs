using AutoMapper;
using FluentValidation;
using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Students;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Paging;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace LanguageCourseManagement.Application.Services.StudentService;

public sealed class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<StudentService> _logger;
    private readonly IValidator<CreateStudentRequest> _createValidator;
    private readonly IValidator<UpdateStudentRequest> _updateValidator;

    public StudentService(
        IStudentRepository studentRepository,
        IMapper mapper,
        ILogger<StudentService> logger,
        IValidator<CreateStudentRequest> createValidator,
        IValidator<UpdateStudentRequest> updateValidator)
    {
        _studentRepository = studentRepository;
        _mapper = mapper;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<StudentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var student = await _studentRepository.GetAsync(
            item => item.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken);

        if (student is null)
            throw new NotFoundException("Öğrenci bulunamadı.");

        return _mapper.Map<StudentResponse>(student);
    }

    public async Task<GetListResponse<StudentListResponse>> GetListAsync(
        PageRequest pageRequest,
        string? search,
        bool? isActive,
        bool showDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        var searchLower = normalizedSearch?.ToLowerInvariant();
        Expression<Func<Student, bool>> predicate = student =>
            (!isActive.HasValue || student.IsActive == isActive.Value) &&
            (searchLower == null ||
             student.FirstName.ToLower().Contains(searchLower) ||
             student.LastName.ToLower().Contains(searchLower) ||
             student.MobilePhone.ToLower().Contains(searchLower) ||
             (student.Email != null && student.Email.ToLower().Contains(searchLower)));

        IPaginate<Student> students;
        if (showDeleted)
        {
            var queryable = _studentRepository.QueryWithIgnoreFilters().AsNoTracking();
            queryable = queryable.Where(predicate);
            students = await queryable
                .OrderBy(student => student.LastName).ThenBy(student => student.FirstName)
                .ToPaginateAsync(pageRequest.PageIndex, pageRequest.PageSize, from: 0, cancellationToken);
        }
        else
        {
            students = await _studentRepository.GetListAsync(
                predicate: predicate,
                orderBy: query => query.OrderBy(student => student.LastName).ThenBy(student => student.FirstName),
                index: pageRequest.PageIndex,
                size: pageRequest.PageSize,
                enableTracking: false,
                cancellationToken: cancellationToken);
        }

        return new GetListResponse<StudentListResponse>
        {
            Index = students.Index,
            Size = students.Size,
            Count = students.Count,
            Pages = students.Pages,
            HasPrevious = students.HasPrevious,
            HasNext = students.HasNext,
            Items = students.Items.Select(_mapper.Map<StudentListResponse>).ToList()
        };
    }

    public async Task<StudentResponse> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_createValidator, request, cancellationToken);
        request.FirstName = request.FirstName.Trim();
        request.LastName = request.LastName.Trim();

        var student = _mapper.Map<Student>(request);
        student.Id = Guid.NewGuid();
        student.RegistrationDate = DateTime.UtcNow;
        student.IsActive = true;
        await _studentRepository.AddAsync(student, cancellationToken);

        _logger.LogInformation("[StudentService] Yeni öğrenci oluşturuldu - {StudentId}", student.Id);
        return await GetByIdAsync(student.Id, cancellationToken);
    }

    public async Task<StudentResponse> UpdateAsync(Guid id, UpdateStudentRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_updateValidator, request, cancellationToken);
        request.FirstName = request.FirstName.Trim();
        request.LastName = request.LastName.Trim();

        var student = await _studentRepository.GetAsync(item => item.Id == id, cancellationToken: cancellationToken);
        if (student is null)
            throw new NotFoundException("Öğrenci bulunamadı.");

        student.FirstName = request.FirstName;
        student.LastName = request.LastName;
        student.HomePhone = request.HomePhone;
        student.MobilePhone = request.MobilePhone;
        student.Email = request.Email;
        student.IsActive = request.IsActive;
        await _studentRepository.UpdateAsync(student, cancellationToken);

        _logger.LogInformation("[StudentService] Öğrenci güncellendi - {StudentId}", student.Id);
        return await GetByIdAsync(student.Id, cancellationToken);
    }

    public async Task<StudentResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var student = await _studentRepository.GetAsync(item => item.Id == id, cancellationToken: cancellationToken);
        if (student is null)
            throw new NotFoundException("Öğrenci bulunamadı.");

        var response = _mapper.Map<StudentResponse>(student);
        await _studentRepository.DeleteAsync(student, cancellationToken);

        _logger.LogInformation("[StudentService] Öğrenci silindi - {StudentId}", student.Id);
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
                Property = group.Key,
                Errors = group.Select(error => error.ErrorMessage).ToArray()
            })
            .ToArray());
    }
}
