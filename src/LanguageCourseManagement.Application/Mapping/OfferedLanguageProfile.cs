using AutoMapper;
using LanguageCourseManagement.Application.DTOs.OfferedLanguages;
using LanguageCourseManagement.Domain.Entities;

namespace LanguageCourseManagement.Application.Mapping;

public sealed class OfferedLanguageProfile : Profile
{
    public OfferedLanguageProfile()
    {
        CreateMap<OfferedLanguage, OfferedLanguageListResponse>();
        CreateMap<OfferedLanguage, OfferedLanguageResponse>();

        CreateMap<CreateOfferedLanguageRequest, OfferedLanguage>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore())
            .ForMember(destination => destination.IsDeleted, options => options.Ignore())
            .ForMember(destination => destination.DeletedAt, options => options.Ignore())
            .ForMember(destination => destination.CourseLevels, options => options.Ignore())
            .ForMember(destination => destination.TeacherLanguages, options => options.Ignore())
            .ForMember(destination => destination.Courses, options => options.Ignore());

        CreateMap<UpdateOfferedLanguageRequest, OfferedLanguage>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore())
            .ForMember(destination => destination.IsDeleted, options => options.Ignore())
            .ForMember(destination => destination.DeletedAt, options => options.Ignore())
            .ForMember(destination => destination.CourseLevels, options => options.Ignore())
            .ForMember(destination => destination.TeacherLanguages, options => options.Ignore())
            .ForMember(destination => destination.Courses, options => options.Ignore());
    }
}
