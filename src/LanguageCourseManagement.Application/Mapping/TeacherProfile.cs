using AutoMapper;
using LanguageCourseManagement.Application.DTOs.Teachers;
using LanguageCourseManagement.Domain.Entities;

namespace LanguageCourseManagement.Application.Mapping;

public sealed class TeacherProfile : Profile
{
    public TeacherProfile()
    {
        CreateMap<Teacher, TeacherListResponse>();
        CreateMap<Teacher, TeacherResponse>()
            .ForMember(destination => destination.LanguageIds, options => options.Ignore())
            .ForMember(destination => destination.BranchIds, options => options.Ignore())
            .ForMember(destination => destination.Availabilities, options => options.Ignore());

        CreateMap<CreateTeacherRequest, Teacher>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore())
            .ForMember(destination => destination.IsDeleted, options => options.Ignore())
            .ForMember(destination => destination.DeletedAt, options => options.Ignore())
            .ForMember(destination => destination.TeacherLanguages, options => options.Ignore())
            .ForMember(destination => destination.TeacherBranches, options => options.Ignore())
            .ForMember(destination => destination.Availabilities, options => options.Ignore())
            .ForMember(destination => destination.Courses, options => options.Ignore());

        CreateMap<UpdateTeacherRequest, Teacher>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore())
            .ForMember(destination => destination.IsDeleted, options => options.Ignore())
            .ForMember(destination => destination.DeletedAt, options => options.Ignore())
            .ForMember(destination => destination.TeacherLanguages, options => options.Ignore())
            .ForMember(destination => destination.TeacherBranches, options => options.Ignore())
            .ForMember(destination => destination.Availabilities, options => options.Ignore())
            .ForMember(destination => destination.Courses, options => options.Ignore());
    }
}
