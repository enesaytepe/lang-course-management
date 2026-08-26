using AutoMapper;
using FluentValidation;
using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Application.DTOs.Payments;
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
    private readonly IPaymentRepository _paymentRepository;
    private readonly IValidator<EnrollmentCreateRequest> _createValidator;
    private readonly IValidator<UpdateEnrollmentRequest> _updateValidator;
    private readonly IMapper _mapper;

    public EnrollmentService(
        IEnrollmentRepository enrollmentRepository,
        IPaymentRepository paymentRepository,
        IValidator<EnrollmentCreateRequest> createValidator,
        IValidator<UpdateEnrollmentRequest> updateValidator,
        IMapper mapper)
    {
        _enrollmentRepository = enrollmentRepository;
        _paymentRepository = paymentRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<EnrollmentDetailResponse> RegisterAndSettleAsync(
        EnrollmentCreateRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new Exceptions.ValidationException(validationResult.Errors.Select(f => new ValidationExceptionModel
            {
                Property = f.PropertyName,
                Errors = new[] { f.ErrorMessage }
            }));

        var idempotencyKey = request.IdempotencyKey.Trim();

        // Idempotency check: find existing payment with the same key
        var replay = await _paymentRepository.FindByIdempotencyKeyAsync(idempotencyKey, cancellationToken);

        if (replay is not null)
        {
            var replayEnrollment = replay.Enrollment;
            if (replayEnrollment.StudentId != request.StudentId ||
                replayEnrollment.CourseId != request.CourseId ||
                replayEnrollment.TuitionFee - request.DiscountAmount != replay.Amount)
            {
                throw new BusinessException("İdempotensi anahtarı farklı bir tahsilatla zaten ilişkilendirilmiş.");
            }

            replayEnrollment.Payments ??= new List<Payment>();
            replayEnrollment.Payments.Add(replay);
            return _mapper.Map<EnrollmentDetailResponse>(replayEnrollment);
        }

        // Validate course
        var course = await _enrollmentRepository.GetCourseForSettlementAsync(request.CourseId, cancellationToken)
            ?? throw new NotFoundException("Ders bulunamadı.");

        if (!course.IsActive || course.Status != CourseStatus.Open)
            throw new BusinessException("Seçilen ders kullanıma uygun değil.");

        // Validate student
        var student = await _enrollmentRepository.GetActiveStudentAsync(request.StudentId, cancellationToken)
            ?? throw new NotFoundException("Aktif öğrenci bulunamadı.");

        // Duplicate enrollment check
        if (await _enrollmentRepository.FindByStudentAndCourseAsync(request.StudentId, request.CourseId, cancellationToken) is not null)
            throw new BusinessException("Öğrenci bu derse zaten kayıtlı.");

        // Capacity check
        var activeCount = await _enrollmentRepository.CountActiveByCourseIdAsync(course.Id, cancellationToken);

        if (activeCount >= course.Capacity)
            throw new BusinessException("Ders kontenjanı dolu.");

        if (request.DiscountAmount > course.TuitionFee)
            throw new BusinessException("İndirim tutarı ders ücretini aşamaz.");

        // Create enrollment
        var enrollment = _mapper.Map<Enrollment>(request);
        enrollment.Id = Guid.NewGuid();
        enrollment.EnrollmentDate = DateTime.UtcNow;
        enrollment.RegisteredByUserId = userId;
        enrollment.TuitionFee = course.TuitionFee;
        enrollment.FinalAmount = course.TuitionFee - request.DiscountAmount;
        enrollment.Status = EnrollmentStatus.Active;

        await _enrollmentRepository.AddAsync(enrollment, cancellationToken);

        // Cash: create immediate settled payment
        if (request.PaymentType == PaymentType.Cash)
        {
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                EnrollmentId = enrollment.Id,
                Amount = enrollment.FinalAmount,
                Method = PaymentMethod.Cash,
                Status = PaymentStatus.Settled,
                SettledAt = DateTimeOffset.UtcNow,
                CollectedByUserId = userId,
                IdempotencyKey = idempotencyKey,
                PaymentDate = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(payment, cancellationToken);
            enrollment.Payments ??= new List<Payment>();
            enrollment.Payments.Add(payment);
        }

        return _mapper.Map<EnrollmentDetailResponse>(enrollment);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EnrollmentListItemResponse>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var enrollments = await _enrollmentRepository.GetListWithIncludesAsync(cancellationToken);
        return enrollments.Select(_mapper.Map<EnrollmentListItemResponse>).ToList();
    }

    /// <inheritdoc />
    public async Task<GetListResponse<EnrollmentListItemResponse>> GetListAsync(
        PageRequest pageRequest,
        string? search,
        Guid? branchId,
        EnrollmentStatus? status,
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

        IPaginate<Enrollment> enrollmentsPage = await _enrollmentRepository.GetListAsync(
            predicate: predicate,
            orderBy: query => query.OrderByDescending(enrollment => enrollment.EnrollmentDate),
            include: query => query
                .Include(enrollment => enrollment.Student)
                .Include(enrollment => enrollment.Course)
                .Include(enrollment => enrollment.Payments),
            index: pageRequest.PageIndex,
            size: pageRequest.PageSize,
            enableTracking: false,
            cancellationToken: cancellationToken);

        return new GetListResponse<EnrollmentListItemResponse>
        {
            Index = enrollmentsPage.Index,
            Size = enrollmentsPage.Size,
            Count = enrollmentsPage.Count,
            Pages = enrollmentsPage.Pages,
            HasPrevious = enrollmentsPage.HasPrevious,
            HasNext = enrollmentsPage.HasNext,
            Items = enrollmentsPage.Items.Select(_mapper.Map<EnrollmentListItemResponse>).ToList()
        };
    }

    /// <inheritdoc />
    public async Task<EnrollmentDetailResponse> GetDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var enrollment = await _enrollmentRepository.GetDetailsWithIncludesAsync(id, cancellationToken)
            ?? throw new NotFoundException("Kayıt bulunamadı.");

        return _mapper.Map<EnrollmentDetailResponse>(enrollment);
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
                .Include(item => item.Payments),
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
    public async Task<IReadOnlyList<SettlementResponse>> GetPaymentsAsync(CancellationToken cancellationToken = default)
    {
        var payments = await _paymentRepository.GetListWithIncludesAsync(cancellationToken);
        return payments.Select(_mapper.Map<SettlementResponse>).ToList();
    }

    /// <inheritdoc />
    public async Task<SettlementResponse> GetPaymentDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetDetailsWithIncludesAsync(id, cancellationToken)
            ?? throw new NotFoundException("Tahsilat bulunamadı.");

        return _mapper.Map<SettlementResponse>(payment);
    }
}
