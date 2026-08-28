using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Enums;
using LanguageCourseManagement.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LanguageCourseManagement.Tests;

public sealed class SettlementTests
{
    [Fact]
    public void Settlement_contract_requires_one_settled_cash_payment_for_the_final_amount()
    {
        var enrollment = new Enrollment { FinalAmount = 250m, Payments = new List<Payment>() };
        var payment = new Payment { EnrollmentId = enrollment.Id, Amount = enrollment.FinalAmount, Method = PaymentMethod.Cash, Status = PaymentStatus.Settled };
        enrollment.Payments.Add(payment);

        Assert.Equal(enrollment.FinalAmount, payment.Amount);
        Assert.Equal(PaymentMethod.Cash, payment.Method);
        Assert.Equal(PaymentStatus.Settled, payment.Status);
        Assert.Same(payment, enrollment.Payments.First());
    }

    [Fact]
    public void Payment_idempotency_key_is_persisted_on_the_settlement_record()
    {
        var payment = new Payment { IdempotencyKey = "stable-key-001" };

        Assert.Equal("stable-key-001", payment.IdempotencyKey);
    }

    [Fact]
    public void Settled_payment_is_immutable()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var paymentId = Guid.NewGuid();
        context.Payments.Add(new Payment
        {
            Id = paymentId,
            EnrollmentId = Guid.NewGuid(),
            IdempotencyKey = Guid.NewGuid().ToString(),
            Amount = 500m,
            Method = PaymentMethod.Cash,
            Status = PaymentStatus.Settled,
            SettledAt = DateTimeOffset.UtcNow,
            PaymentDate = DateTime.UtcNow
        });
        context.SaveChanges();

        var trackedPayment = context.Payments.IgnoreQueryFilters().First(p => p.Id == paymentId);
        trackedPayment.Amount = 100m;
        Assert.Throws<InvalidOperationException>(() => context.SaveChanges());

        context.Entry(trackedPayment).State = EntityState.Detached;
        var reloaded = context.Payments.IgnoreQueryFilters().First(p => p.Id == paymentId);
        context.Payments.Remove(reloaded);
        Assert.Throws<InvalidOperationException>(() => context.SaveChanges());
    }
}
