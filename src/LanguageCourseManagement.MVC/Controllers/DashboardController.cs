using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Services.BranchService;
using LanguageCourseManagement.Application.Services.ClassroomService;
using LanguageCourseManagement.Application.Services.CourseService;
using LanguageCourseManagement.Application.Services.EnrollmentService;
using LanguageCourseManagement.Application.Services.StudentService;
using LanguageCourseManagement.Application.Services.TeacherService;
using LanguageCourseManagement.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageCourseManagement.MVC.Controllers;

/// <summary>
/// Yönetim paneli endpoint'leri.
/// </summary>
[Authorize(Roles = "SystemAdmin,RegistrationOfficer")]
public sealed class DashboardController : Controller
{
    private readonly IBranchService _branchService;
    private readonly IClassroomService _classroomService;
    private readonly ITeacherService _teacherService;
    private readonly IStudentService _studentService;
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;

    public DashboardController(
        IBranchService branchService,
        IClassroomService classroomService,
        ITeacherService teacherService,
        IStudentService studentService,
        ICourseService courseService,
        IEnrollmentService enrollmentService)
    {
        _branchService = branchService;
        _classroomService = classroomService;
        _teacherService = teacherService;
        _studentService = studentService;
        _courseService = courseService;
        _enrollmentService = enrollmentService;
    }

    /// <summary>
    /// Yönetim panelini görüntüler.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var pageRequest = new PageRequest { PageIndex = 0, PageSize = 10000 };

        var branches = await _branchService.GetListAsync(
            pageRequest,
            search: null,
            isActive: true,
            cancellationToken: cancellationToken);
        var classrooms = await _classroomService.GetListAsync(pageRequest, search: null, branchId: null, isActive: true, cancellationToken: cancellationToken);
        var teachers = await _teacherService.GetListAsync(pageRequest, search: null, isActive: true, cancellationToken: cancellationToken);
        var students = await _studentService.GetListAsync(pageRequest, search: null, isActive: true, cancellationToken: cancellationToken);
        var courses = await _courseService.GetListAsync(pageRequest, search: null, branchId: null, offeredLanguageId: null, isActive: true, cancellationToken: cancellationToken);
        var enrollments = await _enrollmentService.GetListAsync(cancellationToken);
        var payments = await _enrollmentService.GetPaymentsAsync(cancellationToken);

        return View(new DashboardViewModel
        {
            ActiveBranchCount = branches.Count,
            ActiveClassroomCount = classrooms.Count,
            ActiveTeacherCount = teachers.Count,
            ActiveStudentCount = students.Count,
            ActiveCourseCount = courses.Count,
            TotalEnrollmentCount = enrollments.Count,
            TotalSettledAmount = payments
                .Where(payment => string.Equals(payment.Status, "Settled", StringComparison.OrdinalIgnoreCase))
                .Sum(payment => payment.Amount),
            PendingPaymentCount = enrollments.Count(enrollment => !enrollment.IsSettled)
        });
    }
}
