using AutoMapper;
using LanguageCourseManagement.Application.DTOs.Payments;
using LanguageCourseManagement.Domain.Entities;

namespace LanguageCourseManagement.Application.Mapping;
public sealed class PaymentProfile : Profile
{
    public PaymentProfile() => CreateMap<Payment, SettlementResponse>()
        .ForMember(dest => dest.Method, o => o.MapFrom(x => x.Method.ToString()))
        .ForMember(dest => dest.Status, o => o.MapFrom(x => x.Status.ToString()));
}
