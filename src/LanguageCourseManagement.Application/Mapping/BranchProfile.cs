using AutoMapper;
using LanguageCourseManagement.Application.DTOs.Branches;
using LanguageCourseManagement.Domain.Entities;

namespace LanguageCourseManagement.Application.Mapping;

public sealed class BranchProfile : Profile
{
    public BranchProfile()
    {
        CreateMap<Branch, BranchListResponse>();
        CreateMap<Branch, BranchResponse>()
            .ForMember(destination => destination.FacilityIds, options => options.Ignore());

        CreateMap<CreateBranchRequest, Branch>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore())
            .ForMember(destination => destination.IsDeleted, options => options.Ignore())
            .ForMember(destination => destination.DeletedAt, options => options.Ignore())
            .ForMember(destination => destination.BranchFacilities, options => options.Ignore())
            .ForMember(destination => destination.Classrooms, options => options.Ignore())
            .ForMember(destination => destination.TeacherBranches, options => options.Ignore())
            .ForMember(destination => destination.Courses, options => options.Ignore());

        CreateMap<UpdateBranchRequest, Branch>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore())
            .ForMember(destination => destination.IsDeleted, options => options.Ignore())
            .ForMember(destination => destination.DeletedAt, options => options.Ignore())
            .ForMember(destination => destination.BranchFacilities, options => options.Ignore())
            .ForMember(destination => destination.Classrooms, options => options.Ignore())
            .ForMember(destination => destination.TeacherBranches, options => options.Ignore())
            .ForMember(destination => destination.Courses, options => options.Ignore());
    }
}
