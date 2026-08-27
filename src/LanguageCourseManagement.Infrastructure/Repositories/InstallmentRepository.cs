using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Repositories;

namespace LanguageCourseManagement.Infrastructure.Repositories;

public class InstallmentRepository : EfRepositoryBase<Installment, AppDbContext>, IInstallmentRepository
{
    public InstallmentRepository(AppDbContext context) : base(context) { }
}
