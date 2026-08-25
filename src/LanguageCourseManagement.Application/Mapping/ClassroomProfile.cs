using AutoMapper;
using LanguageCourseManagement.Application.DTOs.Classrooms;
using LanguageCourseManagement.Domain.Entities;

namespace LanguageCourseManagement.Application.Mapping;

public sealed class ClassroomProfile : Profile
{
    public ClassroomProfile()
    {
        CreateMap<Classroom, ClassroomListResponse>()
            .ForMember(
                destination => destination.BranchName,
                options => options.MapFrom(source => source.Branch.Name));

        CreateMap<Classroom, ClassroomResponse>()
            .ForMember(
                destination => destination.BranchName,
                options => options.MapFrom(source => source.Branch.Name));

        CreateMap<CreateClassroomRequest, Classroom>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore())
            .ForMember(destination => destination.IsDeleted, options => options.Ignore())
            .ForMember(destination => destination.DeletedAt, options => options.Ignore())
            .ForMember(destination => destination.IsActive, options => options.Ignore())
            .ForMember(destination => destination.Branch, options => options.Ignore())
            .ForMember(destination => destination.Courses, options => options.Ignore());

        CreateMap<UpdateClassroomRequest, Classroom>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore())
            .ForMember(destination => destination.IsDeleted, options => options.Ignore())
            .ForMember(destination => destination.DeletedAt, options => options.Ignore())
            .ForMember(destination => destination.IsActive, options => options.Ignore())
            .ForMember(destination => destination.Branch, options => options.Ignore())
            .ForMember(destination => destination.Courses, options => options.Ignore());
    }
}
