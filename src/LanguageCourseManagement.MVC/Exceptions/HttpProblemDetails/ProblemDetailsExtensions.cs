using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace LanguageCourseManagement.MVC.Exceptions.HttpProblemDetails;

internal static class ProblemDetailsExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = null // PascalCase explicit
    };

    public static string AsJson<TProblemDetail>(this TProblemDetail details)
        where TProblemDetail : ProblemDetails
    {
        return JsonSerializer.Serialize(details, _jsonOptions);
    }
}
