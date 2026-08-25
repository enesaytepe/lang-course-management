namespace LanguageCourseManagement.MVC.Models.ViewModels;

public sealed class DashboardViewModel
{
    public int ActiveBranchCount { get; init; }
    public int ActiveClassroomCount { get; init; }
    public int ActiveTeacherCount { get; init; }
    public int ActiveStudentCount { get; init; }
    public int ActiveCourseCount { get; init; }
    public int TotalEnrollmentCount { get; init; }
    public decimal TotalSettledAmount { get; init; }
    public int PendingPaymentCount { get; init; }
}
