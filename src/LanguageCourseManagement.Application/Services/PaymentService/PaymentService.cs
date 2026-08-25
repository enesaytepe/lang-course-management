using AutoMapper;
using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Payments;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Enums;
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
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IEnrollmentRepository enrollmentRepository,
        IMapper mapper,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _enrollmentRepository = enrollmentRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaymentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Payment? payment = await _paymentRepository.GetAsync(
            p => p.Id == id,
            include: q => q
                .Include(p => p.Enrollment)
                    .ThenInclude(e => e.Student)
                .Include(p => p.Enrollment)
                    .ThenInclude(e => e.Course)
                        .ThenInclude(c => c.Branch),
            cancellationToken: cancellationToken);

        if (payment is null)
            throw new NotFoundException("Tahsilat bulunamadı.");

        _logger.LogInformation("[PaymentService] Tahsilat detay getirildi - {PaymentId}", id);
        return ToResponse(payment);
    }

    /// <inheritdoc />
    public async Task<GetListResponse<PaymentListResponse>> GetListAsync(
        PageRequest pageRequest,
        string? search,
        CancellationToken cancellationToken = default)
    {
        Expression<Func<Payment, bool>>? predicate = null;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLowerInvariant();
            predicate = p =>
                p.Enrollment.Student.FirstName.ToLower().Contains(searchLower) ||
                p.Enrollment.Student.LastName.ToLower().Contains(searchLower) ||
                p.Enrollment.Course.Name.ToLower().Contains(searchLower);
        }

        var payments = await _paymentRepository.GetListAsync(
            predicate: predicate,
            orderBy: q => q.OrderByDescending(p => p.SettledAt),
            include: q => q
                .Include(p => p.Enrollment)
                    .ThenInclude(e => e.Student)
                .Include(p => p.Enrollment)
                    .ThenInclude(e => e.Course)
                        .ThenInclude(c => c.Branch),
            index: pageRequest.PageIndex,
            size: pageRequest.PageSize,
            cancellationToken: cancellationToken);

        _logger.LogInformation("[PaymentService] Tahsilat listesi getirildi - Sayfa: {PageIndex}, Boyut: {PageSize}", pageRequest.PageIndex, pageRequest.PageSize);
        return new GetListResponse<PaymentListResponse>
        {
            Index = payments.Index,
            Size = payments.Size,
            Count = payments.Count,
            Pages = payments.Pages,
            HasPrevious = payments.HasPrevious,
            HasNext = payments.HasNext,
            Items = payments.Items.Select(ToListResponse).ToList()
        };
    }

    /// <inheritdoc />
    public async Task<PaymentResponse> CreateAsync(CreatePaymentRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        // Idempotency: Aynı kayıt için birden fazla tahsilat engellenir
        Payment? existingPayment = await _paymentRepository.GetByEnrollmentIdAsync(request.EnrollmentId, cancellationToken);
        if (existingPayment is not null)
            throw new BusinessException("Bu kayıt için tahsilat zaten yapılmıştır.");

        // Kayıt doğrulaması
        Enrollment? enrollment = await _enrollmentRepository.GetAsync(
            e => e.Id == request.EnrollmentId,
            include: q => q
                .Include(e => e.Student)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Branch),
            cancellationToken: cancellationToken);

        if (enrollment is null)
            throw new NotFoundException("Kayıt bulunamadı.");

        if (enrollment.Status != EnrollmentStatus.Active)
            throw new BusinessException("Yalnızca aktif kayıtlar için tahsilat yapılabilir.");

        // İlk sürümde sadece nakit tam tahsilat: Tutar otomatik olarak kaydın nihai tutarına eşitlenir
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            EnrollmentId = request.EnrollmentId,
            Amount = enrollment.FinalAmount,
            Method = PaymentMethod.Cash,
            Status = PaymentStatus.Settled,
            SettledAt = DateTimeOffset.UtcNow,
            CollectedByUserId = userId,
            IdempotencyKey = Guid.NewGuid().ToString("N"),
            Description = request.Description
        };

        await _paymentRepository.AddAsync(payment);
        _logger.LogInformation("[PaymentService] Yeni tahsilat olusturuldu - {PaymentId}, Kayit: {EnrollmentId}, Tutar: {Amount}", payment.Id, request.EnrollmentId, payment.Amount);

        // Navigation'ları response için doldur
        payment.Enrollment = enrollment;
        return ToResponse(payment);
    }

    private static PaymentResponse ToResponse(Payment payment)
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
            Status = payment.Status.ToString(),
            SettledAt = payment.SettledAt
        };
    }
}
