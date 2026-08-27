namespace LanguageCourseManagement.Domain.Enums;

public static class CourseStatusExtensions
{
    private static readonly Dictionary<CourseStatus, string> DisplayNames = new()
    {
        [CourseStatus.Draft] = "Taslak",
        [CourseStatus.Open] = "Açık",
        [CourseStatus.Completed] = "Tamamlandı",
        [CourseStatus.Cancelled] = "İptal"
    };

    public static string ToDisplayString(this CourseStatus status)
    {
        return DisplayNames.TryGetValue(status, out var name) ? name : status.ToString();
    }
}
