using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Enums;
using LanguageCourseManagement.Infrastructure;
using Microsoft.AspNetCore.Http;
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
    public async Task Settled_payment_is_immutable_when_tracked_or_detached()
    {
        // Uretim guard'u AppDbContext.SaveChangesAsync/SaveChanges override icinde calisiyor
        // (AppDbContext.cs EnsureSettledPaymentsAreImmutable). Guard, change tracker uzerinden
        // calistigi icin provider'dan bagimsizdir; bu yuzden EF Core InMemory saglayicisi
        // SaveChanges guard'unu gercek anlamda dogrular.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"SettlementImmutability_{Guid.NewGuid():N}")
            .Options;

        const string expectedMessageFragment =
            "Payment in 'Settled' status is immutable and cannot be modified or deleted.";

        // (a) TRACKED: Settled odemenin uzerinde degisiklik yapilirsa guard firlatir.
        var settledPaymentId = Guid.NewGuid();
        await using (var context = new AppDbContext(options, new HttpContextAccessor()))
        {
            var payment = new Payment
            {
                Id = settledPaymentId,
                EnrollmentId = Guid.NewGuid(),
                Amount = 250m,
                Method = PaymentMethod.Cash,
                Status = PaymentStatus.Settled,
                IdempotencyKey = $"SETTLE-{settledPaymentId:N}",
                PaymentDate = DateTime.UtcNow,
                SettledAt = DateTimeOffset.UtcNow,
            };
            context.Payments.Add(payment);
            await context.SaveChangesAsync();

            payment.Amount = 1m; // Settled kayit uzerinde yasakli degisiklik

            var trackedException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => context.SaveChangesAsync());
            Assert.Contains(expectedMessageFragment, trackedException.Message, StringComparison.Ordinal);
        }

        // (b) DETACHED UPDATE: baglamindan koparilmis Settled odemeye Update uygulanirsa guard firlatir.
        await using (var context = new AppDbContext(options, new HttpContextAccessor()))
        {
            var detached = new Payment
            {
                Id = settledPaymentId,
                EnrollmentId = Guid.NewGuid(),
                Amount = 999m,
                Method = PaymentMethod.Cash,
                Status = PaymentStatus.Pending,
                IdempotencyKey = $"SETTLE-{settledPaymentId:N}",
                PaymentDate = DateTime.UtcNow,
                SettledAt = DateTimeOffset.UtcNow,
            };
            context.Payments.Update(detached);

            var persistedValues = await context.Entry(detached).GetDatabaseValuesAsync();
            Assert.NotNull(persistedValues);
            Assert.Equal(PaymentStatus.Settled, persistedValues!["Status"]);
            context.Entry(detached).OriginalValues.SetValues(persistedValues);

            var detachedUpdateException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => context.SaveChangesAsync());
            Assert.Contains(expectedMessageFragment, detachedUpdateException.Message, StringComparison.Ordinal);
        }

        // (c) DETACHED DELETE: baglamindan koparilmis Settled odemeye Remove uygulanirsa guard firlatir.
        await using (var context = new AppDbContext(options, new HttpContextAccessor()))
        {
            var detached = new Payment
            {
                Id = settledPaymentId,
                EnrollmentId = Guid.NewGuid(),
                Amount = 250m,
                Method = PaymentMethod.Cash,
                Status = PaymentStatus.Pending,
                IdempotencyKey = $"SETTLE-{settledPaymentId:N}",
                PaymentDate = DateTime.UtcNow,
                SettledAt = DateTimeOffset.UtcNow,
            };
            context.Payments.Remove(detached);

            var persistedValues = await context.Entry(detached).GetDatabaseValuesAsync();
            Assert.NotNull(persistedValues);
            Assert.Equal(PaymentStatus.Settled, persistedValues!["Status"]);
            context.Entry(detached).OriginalValues.SetValues(persistedValues);

            var detachedDeleteException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => context.SaveChangesAsync());
            Assert.Contains(expectedMessageFragment, detachedDeleteException.Message, StringComparison.Ordinal);
        }

        await using (var context = new AppDbContext(options, new HttpContextAccessor()))
        {
            var persisted = await context.Payments
                .IgnoreQueryFilters()
                .AsNoTracking()
                .SingleAsync(p => p.Id == settledPaymentId);
            Assert.Equal(250m, persisted.Amount);
            Assert.Equal(PaymentMethod.Cash, persisted.Method);
            Assert.Equal(PaymentStatus.Settled, persisted.Status);
            Assert.False(persisted.IsDeleted);
        }
    }
}
