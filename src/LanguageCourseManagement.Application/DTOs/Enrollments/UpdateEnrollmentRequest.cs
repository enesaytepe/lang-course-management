using LanguageCourseManagement.Domain.Enums;

namespace LanguageCourseManagement.Application.DTOs.Enrollments;

public sealed class UpdateEnrollmentRequest
{
    public EnrollmentStatus Status { get; set; }
}
