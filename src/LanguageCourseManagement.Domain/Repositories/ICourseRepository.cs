using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Interfaces;

namespace LanguageCourseManagement.Domain.Repositories;

/// <summary>
/// Kurs veri erişim işlemlerini tanımlar.
/// </summary>
public interface ICourseRepository : IRepository<Course>
{
}
