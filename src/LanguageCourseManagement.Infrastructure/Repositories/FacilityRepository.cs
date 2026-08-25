using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Paging;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LanguageCourseManagement.Infrastructure.Repositories;

public sealed class FacilityRepository : IFacilityRepository
{
    private readonly AppDbContext _context;

    public FacilityRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Facility>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Facilities
            .AsNoTracking()
            .OrderBy(facility => facility.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Facility>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Facilities
            .AsNoTracking()
            .Where(facility => facility.IsActive)
            .OrderBy(facility => facility.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Guid>> GetActiveIdsAsync(
        List<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return [];

        return await _context.Facilities
            .AsNoTracking()
            .Where(facility => facility.IsActive && ids.Contains(facility.Id))
            .Select(facility => facility.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Guid>> GetIdsAsync(
        List<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return [];

        return await _context.Facilities
            .AsNoTracking()
            .Where(facility => ids.Contains(facility.Id))
            .Select(facility => facility.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IPaginate<Facility>> GetListAsync(
        int index,
        int size,
        string? search = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        var query = _context.Facilities
            .AsNoTracking()
            .Where(facility =>
                (!isActive.HasValue || facility.IsActive == isActive.Value) &&
                (normalizedSearch == null ||
                 facility.Name.Contains(normalizedSearch) ||
                 (facility.Description != null && facility.Description.Contains(normalizedSearch))))
            .OrderBy(facility => facility.Name);

        return await query.ToPaginateAsync(index, size, cancellationToken: cancellationToken);
    }

    public Task<Facility?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _context.Facilities
            .FirstOrDefaultAsync(facility => facility.Id == id, cancellationToken);
    }

    public Task<bool> NameExistsAsync(
        string name,
        Guid? excludeFacilityId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLower();

        var query = _context.Facilities
            .AsNoTracking()
            .Where(facility => facility.Name.Trim().ToLower() == normalizedName);

        if (excludeFacilityId.HasValue)
            query = query.Where(facility => facility.Id != excludeFacilityId.Value);

        return query.AnyAsync(cancellationToken);
    }

    public async Task<Facility> AddAsync(
        Facility facility,
        CancellationToken cancellationToken = default)
    {
        await _context.Facilities.AddAsync(facility, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return facility;
    }

    public async Task<Facility> UpdateAsync(
        Facility facility,
        CancellationToken cancellationToken = default)
    {
        _context.Facilities.Update(facility);
        await _context.SaveChangesAsync(cancellationToken);
        return facility;
    }

    public async Task<Facility> DeleteAsync(
        Facility facility,
        CancellationToken cancellationToken = default)
    {
        _context.Facilities.Remove(facility);
        await _context.SaveChangesAsync(cancellationToken);
        return facility;
    }
}
