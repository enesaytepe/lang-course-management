using FluentValidation;
using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.Application.Validators;

public sealed class EnrollmentCreateRequestValidator : AbstractValidator<EnrollmentCreateRequest>
{
    public EnrollmentCreateRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty()
            .WithMessage("Öğrenci seçimi zorunludur.");

        RuleFor(x => x.CourseId)
            .NotEmpty()
            .WithMessage("Ders seçimi zorunludur.");

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("İndirim tutarı negatif olamaz.")
            .LessThanOrEqualTo(1_000_000)
            .WithMessage("İndirim tutarı 1.000.000 TL'yi aşamaz.");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("İdempotensi anahtarı zorunludur.")
            .Must(key => !string.IsNullOrWhiteSpace(key))
            .WithMessage("İdempotensi anahtarı boş olamaz.")
            .Length(8, 100)
            .WithMessage("İdempotensi anahtarı 8 ile 100 karakter arasında olmalıdır.")
            .Matches("^[A-Za-z0-9._:-]+$")
            .WithMessage("İdempotensi anahtarı yalnızca harf, rakam ve (_, ., :, -) karakterleri içerebilir.");

        RuleFor(x => x.PaymentType)
            .IsInEnum()
            .WithMessage("Geçersiz ödeme türü.");
    }
}
