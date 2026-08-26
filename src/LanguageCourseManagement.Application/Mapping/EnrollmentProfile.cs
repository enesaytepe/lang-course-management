using AutoMapper;
using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Application.DTOs.Payments;
using LanguageCourseManagement.Domain.Entities;

namespace LanguageCourseManagement.Application.Mapping;

public sealed class EnrollmentProfile : Profile
{
    public EnrollmentProfile()
    {
        // Entity -> List response
        CreateMap<Enrollment, EnrollmentListItemResponse>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null
                    ? $"{src.Student.FirstName} {src.Student.LastName}"
                    : string.Empty))
            .ForMember(dest => dest.CourseName,
                opt => opt.MapFrom(src => src.Course != null ? src.Course.Name : string.Empty))
            .ForMember(dest => dest.IsSettled,
                opt => opt.MapFrom(src => src.Payments.Any()));

        // Entity -> Detail response
        CreateMap<Enrollment, EnrollmentDetailResponse>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null
                    ? $"{src.Student.FirstName} {src.Student.LastName}"
                    : string.Empty))
            .ForMember(dest => dest.CourseName,
                opt => opt.MapFrom(src => src.Course != null ? src.Course.Name : string.Empty))
            .ForMember(dest => dest.IsSettled,
                opt => opt.MapFrom(src => src.Payments.Any()))
            .ForMember(dest => dest.PaymentId,
                opt => opt.MapFrom(src => src.Payments.FirstOrDefault() != null ? src.Payments.FirstOrDefault()!.Id : (Guid?)null));

        // Request -> Entity (ignore server-controlled fields)
        CreateMap<EnrollmentCreateRequest, Enrollment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EnrollmentDate, opt => opt.Ignore())
            .ForMember(dest => dest.TuitionFee, opt => opt.Ignore())
            .ForMember(dest => dest.FinalAmount, opt => opt.Ignore())
            .ForMember(dest => dest.RegisteredByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.DiscountAmount, opt => opt.Ignore())
            .ForMember(dest => dest.Payments, opt => opt.Ignore())
            .ForMember(dest => dest.Student, opt => opt.Ignore())
            .ForMember(dest => dest.Course, opt => opt.Ignore());

        // Payment entity -> Settlement response
        CreateMap<Payment, SettlementResponse>()
            .ForMember(dest => dest.Method,
                opt => opt.MapFrom(src => src.Method.ToString()))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
