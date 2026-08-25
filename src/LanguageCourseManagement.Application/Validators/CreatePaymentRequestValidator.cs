using FluentValidation;
using LanguageCourseManagement.Application.DTOs.Payments;

namespace LanguageCourseManagement.Application.Validators;

/// <summary>
/// CreatePaymentRequest doğrulama kuralları.
/// </summary>
public sealed class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentRequestValidator()
    {
        RuleFor(x => x.EnrollmentId)
            .NotEmpty()
            .WithMessage("Kayıt seçimi zorunludur.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Açıklama en fazla 500 karakter olabilir.");
    }
}
