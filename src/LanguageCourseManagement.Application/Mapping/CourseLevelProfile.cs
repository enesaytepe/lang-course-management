using AutoMapper;
using LanguageCourseManagement.Application.DTOs.CourseLevels;
using LanguageCourseManagement.Domain.Entities;

namespace LanguageCourseManagement.Application.Mapping;

public sealed class CourseLevelProfile : Profile
{
    public CourseLevelProfile()
    {
        CreateMap<CourseLevel, CourseLevelListResponse>()
            .ForMember(destination => destination.LanguageName, options => options.MapFrom(source => source.OfferedLanguage.Name));
        CreateMap<CourseLevel, CourseLevelResponse>()
            .ForMember(destination => destination.LanguageName, options => options.MapFrom(source => source.OfferedLanguage.Name));

        CreateMap<CreateCourseLevelRequest, CourseLevel>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore())
            .ForMember(destination => destination.IsDeleted, options => options.Ignore())
            .ForMember(destination => destination.DeletedAt, options => options.Ignore())
            .ForMember(destination => destination.OfferedLanguage, options => options.Ignore())
            .ForMember(destination => destination.Courses, options => options.Ignore());

        CreateMap<UpdateCourseLevelRequest, CourseLevel>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore())
            .ForMember(destination => destination.IsDeleted, options => options.Ignore())
            .ForMember(destination => destination.DeletedAt, options => options.Ignore())
            .ForMember(destination => destination.OfferedLanguage, options => options.Ignore())
            .ForMember(destination => destination.Courses, options => options.Ignore());
    }
}
