using AutoMapper;
using LanguageCourseManagement.Application.DTOs.Courses;
using LanguageCourseManagement.Domain.Entities;

namespace LanguageCourseManagement.Application.Mapping;

public sealed class CourseProfile : Profile
{
    public CourseProfile()
    {
        CreateMap<Course, CourseListResponse>()
            .ForMember(destination => destination.BranchName, options => options.MapFrom(source => source.Branch.Name))
            .ForMember(destination => destination.LanguageName, options => options.MapFrom(source => source.OfferedLanguage.Name))
            .ForMember(destination => destination.LevelName, options => options.MapFrom(source => source.CourseLevel.Name))
            .ForMember(destination => destination.TeacherName, options => options.MapFrom(source => source.Teacher.FirstName + " " + source.Teacher.LastName))
            .ForMember(destination => destination.ClassroomName, options => options.MapFrom(source => source.Classroom.Name));

        CreateMap<Course, CourseResponse>()
            .ForMember(destination => destination.BranchName, options => options.MapFrom(source => source.Branch.Name))
            .ForMember(destination => destination.LanguageName, options => options.MapFrom(source => source.OfferedLanguage.Name))
            .ForMember(destination => destination.LevelName, options => options.MapFrom(source => source.CourseLevel.Name))
            .ForMember(destination => destination.TeacherName, options => options.MapFrom(source => source.Teacher.FirstName + " " + source.Teacher.LastName))
            .ForMember(destination => destination.ClassroomName, options => options.MapFrom(source => source.Classroom.Name));

        CreateMap<CreateCourseRequest, Course>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.Branch, options => options.Ignore())
            .ForMember(destination => destination.OfferedLanguage, options => options.Ignore())
            .ForMember(destination => destination.CourseLevel, options => options.Ignore())
            .ForMember(destination => destination.Teacher, options => options.Ignore())
            .ForMember(destination => destination.Classroom, options => options.Ignore())
            .ForMember(destination => destination.Schedules, options => options.Ignore())
            .ForMember(destination => destination.Enrollments, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore());

        CreateMap<UpdateCourseRequest, Course>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.Branch, options => options.Ignore())
            .ForMember(destination => destination.OfferedLanguage, options => options.Ignore())
            .ForMember(destination => destination.CourseLevel, options => options.Ignore())
            .ForMember(destination => destination.Teacher, options => options.Ignore())
            .ForMember(destination => destination.Classroom, options => options.Ignore())
            .ForMember(destination => destination.Schedules, options => options.Ignore())
            .ForMember(destination => destination.Enrollments, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore());
    }
}
