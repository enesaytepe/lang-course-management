using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Exceptions.HttpProblemDetails;

/// <summary>
/// 409 Conflict veritabanı çakışması (unique constraint ihlali) için ProblemDetails yanıtı.
/// </summary>
public class ConflictProblemDetails : ProblemDetails
{
    public ConflictProblemDetails(string detail)
    {
        Title = "Conflict";
        Detail = detail;
        Status = StatusCodes.Status409Conflict;
        Type = "https://api.languagemanagement.edu.tr/problems/conflict";
    }
}
