using LanguageCourseManagement.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Exceptions.HttpProblemDetails;

/// <summary>
/// 422 Unprocessable Entity doğrulama hatası için ProblemDetails yanıtı.
/// </summary>
public class ValidationProblemDetails : ProblemDetails
{
    /// <summary>
    /// Alan adına göre gruplandırılmış doğrulama hataları.
    /// </summary>
    public Dictionary<string, string[]> Errors { get; init; }

    public ValidationProblemDetails(IEnumerable<ValidationExceptionModel> errors)
    {
        Title = "Validation error(s)";
        Detail = "One or more validation errors occurred.";
        Errors = errors
            .GroupBy(e => e.Property ?? string.Empty)
            .ToDictionary(
                g => g.Key,
                g => g.SelectMany(e => e.Errors ?? Enumerable.Empty<string>()).ToArray());
        Status = StatusCodes.Status422UnprocessableEntity;
        Type = "https://example.com/probs/validation";
    }
}
