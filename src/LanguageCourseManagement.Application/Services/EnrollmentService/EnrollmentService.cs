using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Enums;
using LanguageCourseManagement.Domain.Paging;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LanguageCourseManagement.Application.Services.EnrollmentService;

/// <inheritdoc />
public sealed class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IValidator<UpdateEnrollmentRequest> _updateValidator;
    private readonly IMapper _mapper;

    public EnrollmentService(
        IEnrollmentRepository enrollmentRepository,
        IValidator<UpdateEnrollmentRequest> updateValidator,
        IMapper mapper)
    {
        _enrollmentRepository = enrollmentRepository;
        _updateValidator = updateValidator;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EnrollmentListItemResponse>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await _enrollmentRepository.Query()
            .OrderByDescending(e => e.EnrollmentDate)
            .ProjectTo<EnrollmentListItemResponse>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GetListResponse<EnrollmentListItemResponse>> GetListAsync(
        PageRequest pageRequest,
        string? search,
        Guid? branchId,
        EnrollmentStatus? status,
        bool showDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        Expression<Func<Enrollment, bool>> predicate = enrollment =>
            (!branchId.HasValue || enrollment.Course.BranchId == branchId.Value) &&
            (!status.HasValue || enrollment.Status == status.Value) &&
            (normalizedSearch == null ||
             enrollment.Student.FirstName.Contains(normalizedSearch) ||
             enrollment.Student.LastName.Contains(normalizedSearch) ||
             enrollment.Course.Name.Contains(normalizedSearch));

        var enrollmentQuery = showDeleted ? _enrollmentRepository.QueryWithIgnoreFilters() : _enrollmentRepository.Query();
        var enrollmentsPage = await enrollmentQuery
            .Where(predicate)
            .OrderByDescending(enrollment => enrollment.EnrollmentDate)
            .ProjectTo<EnrollmentListItemResponse>(_mapper.ConfigurationProvider)
            .ToPaginateAsync(pageRequest.PageIndex, pageRequest.PageSize, cancellationToken: cancellationToken);

        return new GetListResponse<EnrollmentListItemResponse>
        {
            Index = enrollmentsPage.Index,
            Size = enrollmentsPage.Size,
            Count = enrollmentsPage.Count,
            Pages = enrollmentsPage.Pages,
            HasPrevious = enrollmentsPage.HasPrevious,
            HasNext = enrollmentsPage.HasNext,
            Items = enrollmentsPage.Items.ToList()
        };
    }

    /// <inheritdoc />
    public async Task<EnrollmentDetailResponse> GetDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _enrollmentRepository.Query()
            .Where(e => e.Id == id)
            .ProjectTo<EnrollmentDetailResponse>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Kayıt bulunamadı.");
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EnrollmentListItemResponse>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _enrollmentRepository.Query()
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.EnrollmentDate)
            .ProjectTo<EnrollmentListItemResponse>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<EnrollmentDetailResponse> UpdateStatusAsync(
        Guid id,
        UpdateEnrollmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new Exceptions.ValidationException(validationResult.Errors.Select(f => new ValidationExceptionModel
            {
                Property = f.PropertyName,
                Errors = new[] { f.ErrorMessage }
            }));

        var enrollment = await _enrollmentRepository.GetAsync(
            item => item.Id == id,
            include: query => query
                .Include(item => item.Student)
                .Include(item => item.Course)
                .Include(item => item.Payments!),
            cancellationToken: cancellationToken);

        if (enrollment is null)
            throw new NotFoundException("Kayıt bulunamadı.");

        if (enrollment.Status != EnrollmentStatus.Active)
            throw new BusinessException("Yalnızca aktif kayıtların durumu değiştirilebilir.");

        enrollment.Status = request.Status;
        await _enrollmentRepository.UpdateAsync(enrollment, cancellationToken);

        return _mapper.Map<EnrollmentDetailResponse>(enrollment);
    }

    /// <inheritdoc />
    public Task<EnrollmentDetailResponse> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return UpdateStatusAsync(id, new UpdateEnrollmentRequest { Status = EnrollmentStatus.Cancelled }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<EnrollmentEligibilityResponse> CheckEligibilityAsync(
        Guid studentId,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        // Öğrenci aktif mi kontrol et
        var student = await _enrollmentRepository.GetActiveStudentAsync(studentId, cancellationToken);
        if (student is null)
        {
            return new EnrollmentEligibilityResponse
            {
                IsEligible = false,
                WarningMessage = "Aktif öğrenci bulunamadı."
            };
        }

        // Kurs mevcut mu ve aktif mi kontrol et
        var courseInfo = await _enrollmentRepository.GetCourseEligibilityInfoAsync(courseId, cancellationToken);
        if (courseInfo is null)
        {
            return new EnrollmentEligibilityResponse
            {
                IsEligible = false,
                WarningMessage = "Ders bulunamadı."
            };
        }

        if (!courseInfo.IsActive || courseInfo.Status != CourseStatus.Open)
        {
            return new EnrollmentEligibilityResponse
            {
                IsEligible = false,
                WarningMessage = "Seçilen ders kullanıma uygun değil."
            };
        }

        // Zaten bu derse kayıtlı mı kontrol et (iptal edilmemiş)
        var existingEnrollment = await _enrollmentRepository.FindByStudentAndCourseAsync(studentId, courseId, cancellationToken);
        if (existingEnrollment is not null && existingEnrollment.Status != EnrollmentStatus.Cancelled)
        {
            return new EnrollmentEligibilityResponse
            {
                IsEligible = false,
                WarningMessage = "Öğrenci bu derse zaten kayıtlı.",
                ExistingEnrollmentId = existingEnrollment.Id
            };
        }

        // Kontenjan kontrolü
        var activeCount = await _enrollmentRepository.CountActiveByCourseIdAsync(courseId, cancellationToken);

        if (activeCount >= courseInfo.Capacity)
        {
            return new EnrollmentEligibilityResponse
            {
                IsEligible = false,
                WarningMessage = $"Ders kontenjanı dolu ({activeCount}/{courseInfo.Capacity}).",
                CurrentEnrollmentCount = activeCount,
                CourseCapacity = courseInfo.Capacity
            };
        }

        // Ders programı çakışması kontrolü
        var studentSchedule = await _enrollmentRepository.GetStudentActiveScheduleAsync(studentId, courseId, cancellationToken);

        if (studentSchedule.Count > 0)
        {
            var targetCourseSchedule = await _enrollmentRepository.GetCourseScheduleAsync(courseId, cancellationToken);

            if (targetCourseSchedule.Count > 0)
            {
                var conflictExists = studentSchedule.Any(s =>
                    targetCourseSchedule.Any(t =>
                        t.DayOfWeek == s.DayOfWeek &&
                        t.StartTime < s.EndTime &&
                        t.EndTime > s.StartTime));

                if (conflictExists)
                {
                    return new EnrollmentEligibilityResponse
                    {
                        IsEligible = false,
                        WarningMessage = "Öğrencinin başka bir dersi ile ders programı çakışması var.",
                        CurrentEnrollmentCount = activeCount,
                        CourseCapacity = courseInfo.Capacity
                    };
                }
            }
        }

        return new EnrollmentEligibilityResponse
        {
            IsEligible = true,
            CurrentEnrollmentCount = activeCount,
            CourseCapacity = courseInfo.Capacity
        };
    }
}
