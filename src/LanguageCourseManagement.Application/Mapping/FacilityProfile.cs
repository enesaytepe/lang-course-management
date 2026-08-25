using AutoMapper;
using LanguageCourseManagement.Application.DTOs.Facilities;
using LanguageCourseManagement.Domain.Entities;

namespace LanguageCourseManagement.Application.Mapping;

public sealed class FacilityProfile : Profile
{
    public FacilityProfile()
    {
        CreateMap<Facility, FacilityResponse>();
        CreateMap<Facility, FacilityListResponse>();
    }
}
