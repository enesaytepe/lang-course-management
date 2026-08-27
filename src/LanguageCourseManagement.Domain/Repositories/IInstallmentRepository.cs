using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Interfaces;

namespace LanguageCourseManagement.Domain.Repositories;

/// <summary>
/// Taksit veri erişim işlemlerini tanımlar.
/// </summary>
public interface IInstallmentRepository : IRepository<Installment>
{
}
