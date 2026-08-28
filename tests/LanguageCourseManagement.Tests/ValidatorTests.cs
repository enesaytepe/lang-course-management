using FluentValidation;
using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Application.DTOs.Payments;
using LanguageCourseManagement.Application.Validators;
using LanguageCourseManagement.Domain.Enums;
using Xunit;

namespace LanguageCourseManagement.Tests;

public sealed class ValidatorTests
{
    // EnrollmentCreateRequestValidator tests

    [Fact]
    public void EnrollmentCreateRequestValidator_rejects_empty_student_id()
    {
        var validator = new EnrollmentCreateRequestValidator();
        var result = validator.Validate(new EnrollmentCreateRequest
        {
            StudentId = Guid.Empty,
            CourseId = Guid.NewGuid(),
            DiscountAmount = 0,
            IdempotencyKey = "test-key-1234",
            PaymentType = PaymentType.Cash
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(EnrollmentCreateRequest.StudentId));
    }

    [Fact]
    public void EnrollmentCreateRequestValidator_rejects_empty_course_id()
    {
        var validator = new EnrollmentCreateRequestValidator();
        var result = validator.Validate(new EnrollmentCreateRequest
        {
            StudentId = Guid.NewGuid(),
            CourseId = Guid.Empty,
            DiscountAmount = 0,
            IdempotencyKey = "test-key-1234",
            PaymentType = PaymentType.Cash
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(EnrollmentCreateRequest.CourseId));
    }

    [Fact]
    public void EnrollmentCreateRequestValidator_rejects_negative_discount()
    {
        var validator = new EnrollmentCreateRequestValidator();
        var result = validator.Validate(new EnrollmentCreateRequest
        {
            StudentId = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            DiscountAmount = -1,
            IdempotencyKey = "test-key-1234",
            PaymentType = PaymentType.Cash
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(EnrollmentCreateRequest.DiscountAmount));
    }

    [Fact]
    public void EnrollmentCreateRequestValidator_rejects_discount_over_million()
    {
        var validator = new EnrollmentCreateRequestValidator();
        var result = validator.Validate(new EnrollmentCreateRequest
        {
            StudentId = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            DiscountAmount = 1_000_001,
            IdempotencyKey = "test-key-1234",
            PaymentType = PaymentType.Cash
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(EnrollmentCreateRequest.DiscountAmount));
    }

    [Fact]
    public void EnrollmentCreateRequestValidator_rejects_short_idempotency_key()
    {
        var validator = new EnrollmentCreateRequestValidator();
        var result = validator.Validate(new EnrollmentCreateRequest
        {
            StudentId = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            DiscountAmount = 0,
            IdempotencyKey = "short",
            PaymentType = PaymentType.Cash
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(EnrollmentCreateRequest.IdempotencyKey));
    }

    [Fact]
    public void EnrollmentCreateRequestValidator_rejects_special_chars_in_idempotency_key()
    {
        var validator = new EnrollmentCreateRequestValidator();
        var result = validator.Validate(new EnrollmentCreateRequest
        {
            StudentId = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            DiscountAmount = 0,
            IdempotencyKey = "key with spaces",
            PaymentType = PaymentType.Cash
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(EnrollmentCreateRequest.IdempotencyKey));
    }

    [Fact]
    public void EnrollmentCreateRequestValidator_rejects_invalid_payment_type()
    {
        var validator = new EnrollmentCreateRequestValidator();
        var result = validator.Validate(new EnrollmentCreateRequest
        {
            StudentId = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            DiscountAmount = 0,
            IdempotencyKey = "test-key-1234",
            PaymentType = (PaymentType)999
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(EnrollmentCreateRequest.PaymentType));
    }

    [Fact]
    public void EnrollmentCreateRequestValidator_accepts_valid_request()
    {
        var validator = new EnrollmentCreateRequestValidator();
        var result = validator.Validate(new EnrollmentCreateRequest
        {
            StudentId = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            DiscountAmount = 50,
            IdempotencyKey = "test-key-1234",
            PaymentType = PaymentType.Cash
        });

        Assert.True(result.IsValid);
    }

    // CreatePaymentRequestValidator tests

    [Fact]
    public void CreatePaymentRequestValidator_rejects_empty_enrollment_id()
    {
        var validator = new CreatePaymentRequestValidator();
        var result = validator.Validate(new CreatePaymentRequest
        {
            EnrollmentId = Guid.Empty,
            Method = PaymentMethod.Cash
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreatePaymentRequest.EnrollmentId));
    }

    [Fact]
    public void CreatePaymentRequestValidator_rejects_empty_installment_id_when_present()
    {
        var validator = new CreatePaymentRequestValidator();
        var result = validator.Validate(new CreatePaymentRequest
        {
            EnrollmentId = Guid.NewGuid(),
            InstallmentId = Guid.Empty,
            Method = PaymentMethod.Cash
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreatePaymentRequest.InstallmentId));
    }

    [Fact]
    public void CreatePaymentRequestValidator_accepts_null_installment_id()
    {
        var validator = new CreatePaymentRequestValidator();
        var result = validator.Validate(new CreatePaymentRequest
        {
            EnrollmentId = Guid.NewGuid(),
            InstallmentId = null,
            Method = PaymentMethod.Cash
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreatePaymentRequestValidator_rejects_long_description()
    {
        var validator = new CreatePaymentRequestValidator();
        var result = validator.Validate(new CreatePaymentRequest
        {
            EnrollmentId = Guid.NewGuid(),
            Method = PaymentMethod.Cash,
            Description = new string('x', 501)
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreatePaymentRequest.Description));
    }
}
