using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LanguageCourseManagement.Infrastructure.Repositories;

public sealed class OfferedLanguageRepository
    : EfRepositoryBase<OfferedLanguage, AppDbContext>, IOfferedLanguageRepository
{
    public OfferedLanguageRepository(AppDbContext context) : base(context)
    {
    }

    public Task<bool> NameExistsAsync(
        string name,
        Guid? excludeOfferedLanguageId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLower();

        var query = Context.OfferedLanguages
            .AsNoTracking()
            .Where(language => language.Name.Trim().ToLower() == normalizedName);

        if (excludeOfferedLanguageId.HasValue)
            query = query.Where(language => language.Id != excludeOfferedLanguageId.Value);

        return query.AnyAsync(cancellationToken);
    }
}
