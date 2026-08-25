using FluentValidation;
using LanguageCourseManagement.Application.Mapping;
using LanguageCourseManagement.Application.Services.BranchService;
using LanguageCourseManagement.Application.Services.ClassroomService;
using LanguageCourseManagement.Application.Services.FacilityService;
using LanguageCourseManagement.Application.Services.EnrollmentService;
using LanguageCourseManagement.Application.Services.CourseLevelService;
using LanguageCourseManagement.Application.Services.CourseService;
using LanguageCourseManagement.Application.Services.OfferedLanguageService;
using LanguageCourseManagement.Application.Services.PaymentService;
using LanguageCourseManagement.Application.Services.StudentService;
using LanguageCourseManagement.Application.Services.TeacherService;
using LanguageCourseManagement.Application.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace LanguageCourseManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(_ => { }, typeof(BranchProfile).Assembly);
        services.AddValidatorsFromAssembly(typeof(CreateBranchRequestValidator).Assembly);
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IClassroomService, ClassroomService>();
        services.AddScoped<IFacilityService, FacilityService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<ICourseLevelService, CourseLevelService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IOfferedLanguageService, OfferedLanguageService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ITeacherService, TeacherService>();

        return services;
    }
}
