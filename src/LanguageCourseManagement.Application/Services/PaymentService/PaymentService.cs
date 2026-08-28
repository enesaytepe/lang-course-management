using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Application.DTOs.Payments;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.Persistence;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Enums;
using LanguageCourseManagement.Domain.Paging;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace LanguageCourseManagement.Application.Services.PaymentService;

/// <inheritdoc />
public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IInstallmentRepository _installmentRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<EnrollmentCreateRequest> _createValidator;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IEnrollmentRepository enrollmentRepository,
        IInstallmentRepository installmentRepository,
        ITransactionManager transactionManager,
        IValidator<EnrollmentCreateRequest> createValidator,
        IMapper mapper,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _enrollmentRepository = enrollmentRepository;
        _installmentRepository = installmentRepository;
        _transactionManager = transactionManager;
        _createValidator = createValidator;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<EnrollmentDetailResponse> EnrollWithPaymentAsync(
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

        await _transactionManager.BeginTransactionAsync(cancellationToken);
        try
        {
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
            var activeCount = await _enrollmentRepository.CountActiveByCourseIdForUpdateAsync(course.Id, cancellationToken);

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
                var payment = CreatePaymentRecord(enrollment.Id, enrollment.FinalAmount, PaymentMethod.Cash, userId, idempotencyKey);
                await _paymentRepository.AddAsync(payment, cancellationToken);
                enrollment.Payments ??= new List<Payment>();
                enrollment.Payments.Add(payment);
            }
            else if (request.PaymentType == PaymentType.Installment && request.InstallmentCount.HasValue)
            {
                // Create installment plan
                var installmentCount = Math.Clamp(request.InstallmentCount.Value, 2, 12);
                var installmentAmount = Math.Round(enrollment.FinalAmount / installmentCount, 2);
                var lastInstallmentAmount = enrollment.FinalAmount - (installmentAmount * (installmentCount - 1));
                var baseDate = DateOnly.FromDateTime(enrollment.EnrollmentDate);
                var installments = new List<Installment>();

                for (int i = 1; i <= installmentCount; i++)
                {
                    var amount = i == installmentCount ? lastInstallmentAmount : installmentAmount;
                    installments.Add(new Installment
                    {
                        Id = Guid.NewGuid(),
                        EnrollmentId = enrollment.Id,
                        InstallmentNumber = i,
                        Amount = amount,
                        DueDate = baseDate.AddMonths(i),
                        Status = PaymentStatus.Pending,
                        Description = $"{i}. taksit"
                    });
                }

                enrollment.Installments = installments;
            }

            await _enrollmentRepository.SaveChangesAsync(cancellationToken);
            await _transactionManager.CommitAsync(cancellationToken);

            _logger.LogInformation("[PaymentService] Kayıt ve tahsilat oluşturuldu - Kayıt: {EnrollmentId}, Öğrenci: {StudentId}, Ders: {CourseId}", enrollment.Id, request.StudentId, request.CourseId);

            return _mapper.Map<EnrollmentDetailResponse>(enrollment);
        }
        catch
        {
            await _transactionManager.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SettlementResponse>> GetAllSettlementsAsync(CancellationToken cancellationToken = default)
    {
        return await _paymentRepository.Query()
            .OrderByDescending(p => p.SettledAt)
            .ProjectTo<SettlementResponse>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PaymentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _paymentRepository.Query()
            .Where(p => p.Id == id)
            .ProjectTo<PaymentResponse>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (response is null)
            throw new NotFoundException("Tahsilat bulunamadı.");

        _logger.LogInformation("[PaymentService] Tahsilat detay getirildi - {PaymentId}", id);
        return response;
    }

    /// <inheritdoc />
    public async Task<GetListResponse<PaymentListResponse>> GetListAsync(
        PageRequest pageRequest,
        string? search,
        Guid? branchId = null,
        PaymentStatus? status = null,
        bool showDeleted = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Payment> query = showDeleted ? _paymentRepository.QueryWithIgnoreFilters() : _paymentRepository.Query();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLowerInvariant();
            query = query.Where(p =>
                p.Enrollment.Student.FirstName.ToLower().Contains(searchLower) ||
                p.Enrollment.Student.LastName.ToLower().Contains(searchLower) ||
                p.Enrollment.Course.Name.ToLower().Contains(searchLower));
        }

        if (branchId.HasValue)
        {
            query = query.Where(p => p.Enrollment.Course.BranchId == branchId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        var payments = await query
            .OrderByDescending(p => p.SettledAt)
            .ProjectTo<PaymentListResponse>(_mapper.ConfigurationProvider)
            .ToPaginateAsync(pageRequest.PageIndex, pageRequest.PageSize, cancellationToken: cancellationToken);

        _logger.LogInformation("[PaymentService] Tahsilat listesi getirildi - Sayfa: {PageIndex}, Boyut: {PageSize}", pageRequest.PageIndex, pageRequest.PageSize);
        return new GetListResponse<PaymentListResponse>
        {
            Index = payments.Index,
            Size = payments.Size,
            Count = payments.Count,
            Pages = payments.Pages,
            HasPrevious = payments.HasPrevious,
            HasNext = payments.HasNext,
            Items = payments.Items.ToList()
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PaymentHistoryItem>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _paymentRepository.Query()
            .Where(p => p.Enrollment.StudentId == studentId)
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => new PaymentHistoryItem
            {
                CourseName = p.Enrollment.Course.Name,
                Amount = p.Amount,
                Method = p.Method.ToString(),
                PaymentDate = p.PaymentDate,
                Status = p.Status.ToString()
            })
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PaymentResponse> CreateAsync(CreatePaymentRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        // Kayıt doğrulaması (taksitli ödemeler için RemainingBalance hesaplaması Payments koleksiyonuna bağlıdır)
        Enrollment? enrollment = await _enrollmentRepository.GetAsync(
            e => e.Id == request.EnrollmentId,
            include: q => q
                .Include(e => e.Student)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Branch)
                .Include(e => e.Payments)
                .Include(e => e.Installments!),
            cancellationToken: cancellationToken);

        if (enrollment is null)
            throw new NotFoundException("Kayıt bulunamadı.");

        if (enrollment.Status != EnrollmentStatus.Active)
            throw new BusinessException("Yalnızca aktif kayıtlar için tahsilat yapılabilir.");

        // Nakit ödemede mükerrer tahsilat engeli; taksitli ödemeye izin verilir
        if (enrollment.PaymentType == PaymentType.Cash)
        {
            Payment? existingPayment = await _paymentRepository.GetByEnrollmentIdAsync(request.EnrollmentId, cancellationToken);
            if (existingPayment is not null)
                throw new BusinessException("Bu kayıt için tahsilat zaten yapılmıştır.");
        }

        decimal paymentAmount;
        Guid? installmentId = null;
        int? installmentNumber = null;

        if (enrollment.PaymentType == PaymentType.Installment)
        {
            if (!request.InstallmentId.HasValue)
                throw new BusinessException("Taksitli ödemede taksit Id'si zorunludur.");

            var installment = enrollment.Installments?.FirstOrDefault(i => i.Id == request.InstallmentId.Value)
                ?? throw new NotFoundException("Taksit bulunamadı.");

            if (installment.Status != PaymentStatus.Pending)
                throw new BusinessException("Bu taksit için tahsilat zaten yapılmış veya iptal edilmiş.");

            installment.Status = PaymentStatus.Settled;
            paymentAmount = installment.Amount;
            installmentId = installment.Id;
            installmentNumber = installment.InstallmentNumber;
        }
        else
        {
            decimal totalPaid = enrollment.Payments?.Where(p => p.Status == PaymentStatus.Settled).Sum(p => p.Amount) ?? 0;
            paymentAmount = enrollment.FinalAmount - totalPaid;

            if (paymentAmount <= 0)
                throw new BusinessException("Bu kayıt için tahsilat zaten tamamlanmıştır.");
        }

        var payment = CreatePaymentRecord(request.EnrollmentId, paymentAmount, request.Method, userId, null, installmentId, request.Description);

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _paymentRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("[PaymentService] Yeni tahsilat olusturuldu - {PaymentId}, Kayit: {EnrollmentId}, Tutar: {Amount}", payment.Id, request.EnrollmentId, payment.Amount);

        payment.Enrollment = enrollment;
        return ToResponse(payment, installmentNumber);
    }

    private static PaymentResponse ToResponse(Payment payment, int? installmentNumber = null)
    {
        return new()
        {
            Id = payment.Id,
            EnrollmentId = payment.EnrollmentId,
            StudentName = payment.Enrollment?.Student is not null
            ? $"{payment.Enrollment.Student.FirstName} {payment.Enrollment.Student.LastName}"
            : string.Empty,
            CourseName = payment.Enrollment?.Course?.Name ?? string.Empty,
            BranchName = payment.Enrollment?.Course?.Branch?.Name ?? string.Empty,
            Amount = payment.Amount,
            Method = payment.Method.ToString(),
            Status = payment.Status.ToString(),
            SettledAt = payment.SettledAt,
            Description = payment.Description
        };
    }

    private static PaymentListResponse ToListResponse(Payment payment)
    {
        return new()
        {
            Id = payment.Id,
            StudentName = payment.Enrollment?.Student is not null
            ? $"{payment.Enrollment.Student.FirstName} {payment.Enrollment.Student.LastName}"
            : string.Empty,
            CourseName = payment.Enrollment?.Course?.Name ?? string.Empty,
            BranchName = payment.Enrollment?.Course?.Branch?.Name ?? string.Empty,
            Amount = payment.Amount,
            Method = payment.Method.ToString(),
            Status = payment.Status.ToString(),
            SettledAt = payment.SettledAt,
            InstallmentNumber = payment.Installment?.InstallmentNumber
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EnrollmentOptionDto>> GetUnsettledEnrollmentsAsync(CancellationToken cancellationToken = default)
    {
        return await _enrollmentRepository.Query()
            .Where(e => e.Status == EnrollmentStatus.Active
                && !e.Payments!.Any())
            .OrderByDescending(e => e.EnrollmentDate)
            .Select(e => new EnrollmentOptionDto
            {
                Id = e.Id,
                StudentName = e.Student.FirstName + " " + e.Student.LastName,
                CourseName = e.Course.Name,
                BranchName = e.Course.Branch.Name,
                FinalAmount = e.FinalAmount,
                PaymentType = e.PaymentType.ToString()
            })
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<InstallmentOptionDto>> GetPendingInstallmentsByEnrollmentIdsAsync(
        IReadOnlyList<Guid> enrollmentIds,
        CancellationToken cancellationToken = default)
    {
        if (enrollmentIds.Count == 0)
            return [];

        return await _installmentRepository.Query()
            .Where(i => enrollmentIds.Contains(i.EnrollmentId)
                && i.Status == PaymentStatus.Pending)
            .OrderBy(i => i.EnrollmentId)
            .ThenBy(i => i.InstallmentNumber)
            .Select(i => new InstallmentOptionDto
            {
                EnrollmentId = i.EnrollmentId,
                Id = i.Id,
                InstallmentNumber = i.InstallmentNumber,
                Amount = i.Amount,
                DueDate = i.DueDate,
                Status = i.Status.ToString()
            })
            .ToListAsync(cancellationToken);
    }

    private static Payment CreatePaymentRecord(
        Guid enrollmentId,
        decimal amount,
        PaymentMethod method,
        Guid userId,
        string? idempotencyKey = null,
        Guid? installmentId = null,
        string? description = null)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            EnrollmentId = enrollmentId,
            Amount = amount,
            Method = method,
            Status = PaymentStatus.Settled,
            SettledAt = DateTimeOffset.UtcNow,
            CollectedByUserId = userId,
            IdempotencyKey = idempotencyKey ?? Guid.NewGuid().ToString("N"),
            InstallmentId = installmentId,
            Description = description,
            PaymentDate = DateTime.UtcNow
        };
    }
}
