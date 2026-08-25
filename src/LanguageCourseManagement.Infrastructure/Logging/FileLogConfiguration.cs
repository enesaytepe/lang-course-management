namespace LanguageCourseManagement.Infrastructure.Logging;

public class FileLogConfiguration
{
    public string FolderPath { get; set; } = string.Empty;
    public bool LogBusinessAndValidationExceptions { get; set; }
}
