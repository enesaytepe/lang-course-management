using AutoMapper;
using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Enums;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LanguageCourseManagement.Application.Services.InstallmentService;

/// <inheritdoc />
public sealed class InstallmentService : IInstallmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IInstallmentRepository _installmentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<InstallmentService> _logger;

    public InstallmentService(
        IEnrollmentRepository enrollmentRepository,
        IInstallmentRepository installmentRepository,
        IMapper mapper,
        ILogger<InstallmentService> logger)
    {
        _enrollmentRepository = enrollmentRepository;
        _installmentRepository = installmentRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<InstallmentResponse>> CreateInstallmentPlanAsync(
        Guid enrollmentId,
        int installmentCount,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await _enrollmentRepository.GetAsync(
            e => e.Id == enrollmentId,
            include: q => q
                .Include(e => e.Installments!),
            cancellationToken: cancellationToken)
            ?? throw new NotFoundException("Kayıt bulunamadı.");

        if (enrollment.PaymentType != PaymentType.Installment)
            throw new BusinessException("Bu kayıt taksitli ödeme planına uygun değil.");

        if (enrollment.Status != EnrollmentStatus.Active)
            throw new BusinessException("Taksit planı sadece aktif durumdaki kayıtlar için oluşturulabilir.");

        if (enrollment.Installments != null && enrollment.Installments.Count > 0)
            throw new BusinessException("Bu kayıt için zaten bir taksit planı oluşturulmuş.");

        if (installmentCount < 2 || installmentCount > 12)
            throw new BusinessException("Taksit sayısı 2 ile 12 arasında olmalıdır.");

        var installmentAmount = Math.Round(enrollment.FinalAmount / installmentCount, 2);
        var lastInstallmentAmount = enrollment.FinalAmount - (installmentAmount * (installmentCount - 1));

        var installments = new List<Installment>();
        var baseDate = DateOnly.FromDateTime(enrollment.EnrollmentDate);

        for (int i = 1; i <= installmentCount; i++)
        {
            var amount = i == installmentCount ? lastInstallmentAmount : installmentAmount;
            var dueDate = baseDate.AddMonths(i);

            installments.Add(new Installment
            {
                Id = Guid.NewGuid(),
                EnrollmentId = enrollmentId,
                InstallmentNumber = i,
                Amount = amount,
                DueDate = dueDate,
                Status = PaymentStatus.Pending,
                Description = $"{i}. taksit"
            });
        }

        enrollment.Installments = installments;
        await _enrollmentRepository.UpdateAsync(enrollment, cancellationToken);

        _logger.LogInformation(
            "[InstallmentService] Taksit planı oluşturuldu - Kayıt: {EnrollmentId}, Taksit sayısı: {Count}, Toplam: {Total}",
            enrollmentId, installmentCount, enrollment.FinalAmount);

        return installments.Select(_mapper.Map<InstallmentResponse>).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<InstallmentResponse>> GetByEnrollmentIdAsync(
        Guid enrollmentId,
        CancellationToken cancellationToken = default)
    {
        var enrollmentExists = await _enrollmentRepository.AnyAsync(
            e => e.Id == enrollmentId,
            cancellationToken: cancellationToken);

        if (!enrollmentExists)
            throw new NotFoundException("Kayıt bulunamadı.");

        var installments = await _installmentRepository.Query()
            .Where(i => i.EnrollmentId == enrollmentId)
            .OrderBy(i => i.InstallmentNumber)
            .Select(i => new InstallmentResponse
            {
                Id = i.Id,
                InstallmentNumber = i.InstallmentNumber,
                Amount = i.Amount,
                DueDate = i.DueDate,
                Status = i.Status.ToString(),
                Description = i.Description,
                PaidAmount = i.Payments!
                    .Where(p => p.Status == Domain.Enums.PaymentStatus.Settled)
                    .Sum(p => p.Amount),
                IsPaid = i.Status == Domain.Enums.PaymentStatus.Settled
            })
            .ToListAsync(cancellationToken);

        return installments;
    }

    /// <inheritdoc />
    public async Task MarkOverdueInstallmentsAsync(CancellationToken cancellationToken = default)
    {
        var overdueInstallments = await _installmentRepository.Query()
            .Where(i => i.Status == PaymentStatus.Pending && i.DueDate < DateOnly.FromDateTime(DateTime.Today))
            .ToListAsync(cancellationToken);

        if (overdueInstallments.Count == 0)
            return;

        foreach (var installment in overdueInstallments)
        {
            installment.Status = PaymentStatus.Overdue;
        }

        await _installmentRepository.UpdateRangeAsync(overdueInstallments, cancellationToken);

        _logger.LogInformation(
            "[InstallmentService] {Count} taksit Overdue durumuna geçirildi",
            overdueInstallments.Count);
    }

    /// <inheritdoc />
    public async Task<int> GetOverdueCountAsync(CancellationToken cancellationToken = default)
    {
        return await _installmentRepository.Query()
            .CountAsync(i => i.Status == PaymentStatus.Overdue, cancellationToken);
    }
}
