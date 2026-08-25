using FluentValidation;
using LanguageCourseManagement.Application.DTOs.Branches;

namespace LanguageCourseManagement.Application.Validators;

public sealed class CreateBranchRequestValidator : AbstractValidator<CreateBranchRequest>
{
    public CreateBranchRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().Must(name => !string.IsNullOrWhiteSpace(name))
            .MaximumLength(200);
        RuleFor(request => request.Address).NotEmpty().Must(address => !string.IsNullOrWhiteSpace(address))
            .MaximumLength(500);
        RuleFor(request => request.PublicTransportationDirections).MaximumLength(1000);
        RuleFor(request => request.PrivateVehicleDirections).MaximumLength(1000);
        RuleFor(request => request.PhoneNumber)
            .MaximumLength(32)
            .Must(phone => phone is null || phone.Length == 0
                || (!string.IsNullOrWhiteSpace(phone) && PhoneRegex.IsMatch(phone)))
            .WithMessage("Phone number contains invalid characters.");
        RuleFor(request => request.Latitude).InclusiveBetween(-90m, 90m);
        RuleFor(request => request.Longitude).InclusiveBetween(-180m, 180m);
    }

    private static readonly System.Text.RegularExpressions.Regex PhoneRegex =
        new("^[0-9+\\s()\\-]+$", System.Text.RegularExpressions.RegexOptions.CultureInvariant);
}
