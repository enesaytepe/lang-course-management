using AutoMapper;
using LanguageCourseManagement.Application.DTOs.Branches;
using LanguageCourseManagement.Application.DTOs.Classrooms;
using LanguageCourseManagement.Application.DTOs.CourseLevels;
using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Application.DTOs.OfferedLanguages;
using LanguageCourseManagement.Application.DTOs.Students;
using LanguageCourseManagement.MVC.Models.ViewModels;

namespace LanguageCourseManagement.MVC.Mapping;

public sealed class ViewModelProfile : Profile
{
    public ViewModelProfile()
    {
        // Student mappings
        CreateMap<StudentResponse, StudentDetailsViewModel>();
        CreateMap<StudentFormViewModel, CreateStudentRequest>();
        CreateMap<StudentFormViewModel, UpdateStudentRequest>();

        // OfferedLanguage mappings
        CreateMap<OfferedLanguageResponse, LanguageFormViewModel>();
        CreateMap<LanguageFormViewModel, CreateOfferedLanguageRequest>();
        CreateMap<LanguageFormViewModel, UpdateOfferedLanguageRequest>();

        // Classroom mappings
        CreateMap<ClassroomResponse, ClassroomFormViewModel>()
            .ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.BranchId))
            .ForMember(dest => dest.Branches, opt => opt.Ignore());

        CreateMap<ClassroomFormViewModel, CreateClassroomRequest>()
            .ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.BranchId!.Value));

        CreateMap<ClassroomFormViewModel, UpdateClassroomRequest>()
            .ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.BranchId!.Value));

        // CourseLevel mappings
        CreateMap<CourseLevelResponse, CourseLevelFormViewModel>()
            .ForMember(dest => dest.Languages, opt => opt.Ignore());

        CreateMap<CourseLevelFormViewModel, CreateCourseLevelRequest>()
            .ForMember(dest => dest.OfferedLanguageId, opt => opt.MapFrom(src => src.OfferedLanguageId!.Value));

        CreateMap<CourseLevelFormViewModel, UpdateCourseLevelRequest>()
            .ForMember(dest => dest.OfferedLanguageId, opt => opt.MapFrom(src => src.OfferedLanguageId!.Value));

        // Enrollment mappings
        CreateMap<EnrollmentDetailResponse, EnrollmentDetailViewModel>();

        // Branch mappings
        CreateMap<BranchResponse, BranchFormViewModel>()
            .ForMember(dest => dest.Name, opt => opt.NullSubstitute(string.Empty))
            .ForMember(dest => dest.Address, opt => opt.NullSubstitute(string.Empty))
            .ForMember(dest => dest.FacilityIds, opt => opt.MapFrom(src => src.FacilityIds.ToList()))
            .ForMember(dest => dest.Facilities, opt => opt.Ignore());
    }
}
