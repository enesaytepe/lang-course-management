using AutoMapper;
using LanguageCourseManagement.Application.DTOs.Teachers;
using LanguageCourseManagement.Domain.Entities;

namespace LanguageCourseManagement.Application.Mapping;

public sealed class TeacherAvailabilityProfile : Profile
{
    public TeacherAvailabilityProfile()
    {
        CreateMap<TeacherAvailability, TeacherAvailabilityResponse>();
    }
}
