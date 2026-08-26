using AutoMapper;
using LanguageCourseManagement.Application.DTOs.Payments;
using LanguageCourseManagement.Domain.Entities;

namespace LanguageCourseManagement.Application.Mapping;

public sealed class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<Payment, SettlementResponse>()
            .ForMember(dest => dest.Method, o => o.MapFrom(x => x.Method.ToString()))
            .ForMember(dest => dest.Status, o => o.MapFrom(x => x.Status.ToString()));

        CreateMap<Payment, PaymentResponse>()
            .ForMember(dest => dest.StudentName,
                o => o.MapFrom(x => x.Enrollment.Student.FirstName + " " + x.Enrollment.Student.LastName))
            .ForMember(dest => dest.CourseName,
                o => o.MapFrom(x => x.Enrollment.Course.Name))
            .ForMember(dest => dest.BranchName,
                o => o.MapFrom(x => x.Enrollment.Course.Branch.Name))
            .ForMember(dest => dest.Method,
                o => o.MapFrom(x => x.Method.ToString()))
            .ForMember(dest => dest.Status,
                o => o.MapFrom(x => x.Status.ToString()));

        CreateMap<Payment, PaymentListResponse>()
            .ForMember(dest => dest.StudentName,
                o => o.MapFrom(x => x.Enrollment.Student.FirstName + " " + x.Enrollment.Student.LastName))
            .ForMember(dest => dest.CourseName,
                o => o.MapFrom(x => x.Enrollment.Course.Name))
            .ForMember(dest => dest.BranchName,
                o => o.MapFrom(x => x.Enrollment.Course.Branch.Name))
            .ForMember(dest => dest.Method,
                o => o.MapFrom(x => x.Method.ToString()))
            .ForMember(dest => dest.Status,
                o => o.MapFrom(x => x.Status.ToString()))
            .ForMember(dest => dest.InstallmentNumber,
                o => o.MapFrom(x => x.Installment != null ? x.Installment.InstallmentNumber : (int?)null));
    }
}
