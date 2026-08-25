namespace LanguageCourseManagement.Application.Models;

/// <summary>
/// Universal response for mutation operations (create, update, delete, activate, etc.).
/// Contains the affected entity's identifier.
/// </summary>
public record OperationResult(long Id);
