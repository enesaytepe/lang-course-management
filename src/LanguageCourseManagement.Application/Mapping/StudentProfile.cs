using AutoMapper;
using LanguageCourseManagement.Application.DTOs.Students;
using LanguageCourseManagement.Domain.Entities;

namespace LanguageCourseManagement.Application.Mapping;

public sealed class StudentProfile : Profile
{
    public StudentProfile()
    {
        CreateMap<Student, StudentListResponse>();
        CreateMap<Student, StudentResponse>();

        CreateMap<CreateStudentRequest, Student>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore())
            .ForMember(destination => destination.IsDeleted, options => options.Ignore())
            .ForMember(destination => destination.DeletedAt, options => options.Ignore())
            .ForMember(destination => destination.Enrollments, options => options.Ignore())
            .ForMember(destination => destination.RegistrationDate, options => options.Ignore());

        CreateMap<UpdateStudentRequest, Student>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore())
            .ForMember(destination => destination.IsDeleted, options => options.Ignore())
            .ForMember(destination => destination.DeletedAt, options => options.Ignore())
            .ForMember(destination => destination.Enrollments, options => options.Ignore());
    }
}
