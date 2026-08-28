using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LanguageCourseManagement.Infrastructure.Repositories;

public class BranchRepository : EfRepositoryBase<Branch, AppDbContext>, IBranchRepository
{
    public BranchRepository(AppDbContext context) : base(context) { }

    public async Task<bool> NameExistsAsync(string name, Guid? excludeBranchId = null)
    {
        var query = Context.Branches.AsNoTracking()
            .Where(b => b.Name.ToLower() == name.ToLower());

        // Güncelleme senaryosunda mevcut şubenin kendi ismi çakışma sayılmamalı
        if (excludeBranchId.HasValue)
            query = query.Where(b => b.Id != excludeBranchId.Value);

        return await query.AnyAsync();
    }

    public async Task<Branch> UpdateWithFacilitiesAsync(
        Branch branch,
        List<Guid> facilityIds,
        CancellationToken cancellationToken = default)
    {
        var existingFacilities = await Context.BranchFacilities
            .Where(link => link.BranchId == branch.Id)
            .ToListAsync(cancellationToken);

        Context.BranchFacilities.RemoveRange(existingFacilities);
        var newFacilities = facilityIds
            .Distinct()
            .Select(facilityId => new BranchFacility
            {
                Id = Guid.NewGuid(),
                BranchId = branch.Id,
                FacilityId = facilityId
            })
            .ToList();

        branch.BranchFacilities = newFacilities;
        Context.Entry(branch).State = EntityState.Modified;
        await Context.BranchFacilities.AddRangeAsync(newFacilities, cancellationToken);
        return branch;
    }
}
