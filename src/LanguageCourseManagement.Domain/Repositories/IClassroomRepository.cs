using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Interfaces;

namespace LanguageCourseManagement.Domain.Repositories;

public interface IClassroomRepository : IRepository<Classroom>
{
    Task<bool> NameExistsAsync(
        Guid branchId,
        string name,
        Guid? excludeClassroomId = null,
        CancellationToken cancellationToken = default);
}