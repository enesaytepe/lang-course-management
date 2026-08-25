using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Interfaces;

namespace LanguageCourseManagement.Domain.Repositories;

/// <summary>
/// Öğrenci veri erişim işlemlerini tanımlar.
/// </summary>
public interface IStudentRepository : IRepository<Student>
{
}
