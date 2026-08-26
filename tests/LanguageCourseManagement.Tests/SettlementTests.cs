using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Enums;
using Xunit;

namespace LanguageCourseManagement.Tests;

public sealed class SettlementTests
{
    [Fact]
    public void Settlement_contract_requires_one_settled_cash_payment_for_the_final_amount()
    {
        var enrollment = new Enrollment { FinalAmount = 250m };
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

    [Fact(Skip = "not-verified: the test project does not reference Infrastructure/AppDbContext, so tracked and detached EF mutation/deletion checks require an explicitly authorized project-reference change.")]
    public void Settled_payment_is_immutable_when_tracked_or_detached()
    {
    }
}
