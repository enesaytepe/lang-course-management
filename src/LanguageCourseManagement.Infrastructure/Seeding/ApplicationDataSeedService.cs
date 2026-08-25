using LanguageCourseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace LanguageCourseManagement.Infrastructure.Seeding;

public sealed class ApplicationDataSeedService
{
    private static readonly string[] LevelNames = ["A1", "A2", "B1", "B2", "C1", "C2"];

    private static readonly LanguageSeed[] Languages =
    [
        new("İngilizce", "en"),
        new("Almanca", "de"),
        new("Fransızca", "fr"),
        new("İspanyolca", "es")
    ];

    private static readonly string[] FacilityNames =
    [
        "Kafeterya",
        "Kütüphane",
        "Dinlenme alanı",
        "Otopark"
    ];

    private static readonly BranchSeed[] Branches =
    [
        new(
            "Kadıköy Eğitim Merkezi",
            "Rıhtım Caddesi No: 18, Kadıköy",
            "Metro ve vapur iskelelerine yakın",
            "Yakındaki otoparklar kullanılabilir",
            "0216 555 01 01",
            40.9908m,
            29.0280m,
            ["Kütüphane", "Kafeterya", "Dinlenme alanı"]),
        new(
            "Beşiktaş Eğitim Merkezi",
            "Barbaros Bulvarı No: 42, Beşiktaş",
            "Otobüs duraklarına ve metro bağlantısına yakın",
            "Çevrede ücretli otoparklar bulunur",
            "0212 555 02 02",
            41.0430m,
            29.0050m,
            ["Kafeterya", "Dinlenme alanı", "Otopark"])
    ];

    private static readonly ClassroomSeed[] Classrooms =
    [
        new("Kadıköy Eğitim Merkezi", "Derslik 1", 16),
        new("Kadıköy Eğitim Merkezi", "Derslik 2", 20),
        new("Beşiktaş Eğitim Merkezi", "Derslik 1", 16),
        new("Beşiktaş Eğitim Merkezi", "Derslik 2", 20)
    ];

    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public ApplicationDataSeedService(
        AppDbContext dbContext,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _environment = environment;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await EnsureLanguagesAndLevelsAsync(cancellationToken);

        if (!_environment.IsDevelopment() ||
            !_configuration.GetValue<bool>("Database:SeedDemoData"))
        {
            return;
        }

        await EnsureFacilitiesAsync(cancellationToken);
        await EnsureBranchesAsync(cancellationToken);
        await EnsureClassroomsAsync(cancellationToken);
    }

    private async Task EnsureLanguagesAndLevelsAsync(CancellationToken cancellationToken)
    {
        foreach (var seed in Languages)
        {
            var language = await _dbContext.OfferedLanguages
                .FirstOrDefaultAsync(
                    item => item.Code == seed.Code || item.Name == seed.Name,
                    cancellationToken);

            if (language is null)
            {
                language = new OfferedLanguage
                {
                    Id = Guid.NewGuid(),
                    Name = seed.Name,
                    Code = seed.Code,
                    IsActive = true
                };

                await _dbContext.OfferedLanguages.AddAsync(language, cancellationToken);
            }

            var existingLevels = await _dbContext.CourseLevels
                .Where(level => level.OfferedLanguageId == language.Id)
                .Select(level => new { level.Name, level.Order })
                .ToListAsync(cancellationToken);

            for (var index = 0; index < LevelNames.Length; index++)
            {
                var levelName = LevelNames[index];
                var levelOrder = index + 1;

                if (existingLevels.Any(level =>
                    level.Name == levelName || level.Order == levelOrder))
                {
                    continue;
                }

                await _dbContext.CourseLevels.AddAsync(new CourseLevel
                {
                    Id = Guid.NewGuid(),
                    OfferedLanguageId = language.Id,
                    Name = levelName,
                    Order = levelOrder,
                    IsActive = true
                }, cancellationToken);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureFacilitiesAsync(CancellationToken cancellationToken)
    {
        var existingNames = await _dbContext.Facilities
            .Select(facility => facility.Name)
            .ToListAsync(cancellationToken);

        foreach (var facilityName in FacilityNames)
        {
            if (existingNames.Contains(facilityName))
                continue;

            await _dbContext.Facilities.AddAsync(new Facility
            {
                Id = Guid.NewGuid(),
                Name = facilityName,
                IsActive = true
            }, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureBranchesAsync(CancellationToken cancellationToken)
    {
        var facilities = await _dbContext.Facilities
            .Where(facility => facility.IsActive)
            .ToDictionaryAsync(facility => facility.Name, cancellationToken);

        var existingBranchNames = await _dbContext.Branches
            .Select(branch => branch.Name)
            .ToListAsync(cancellationToken);

        foreach (var seed in Branches)
        {
            if (existingBranchNames.Contains(seed.Name))
                continue;

            var branchId = Guid.NewGuid();
            var branch = new Branch
            {
                Id = branchId,
                Name = seed.Name,
                Address = seed.Address,
                PublicTransportationDirections = seed.PublicTransportationDirections,
                PrivateVehicleDirections = seed.PrivateVehicleDirections,
                PhoneNumber = seed.PhoneNumber,
                Latitude = seed.Latitude,
                Longitude = seed.Longitude,
                IsActive = true,
                BranchFacilities = seed.FacilityNames
                    .Where(facilities.ContainsKey)
                    .Select(facilityName => new BranchFacility
                    {
                        Id = Guid.NewGuid(),
                        BranchId = branchId,
                        FacilityId = facilities[facilityName].Id
                    })
                    .ToList()
            };

            await _dbContext.Branches.AddAsync(branch, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureClassroomsAsync(CancellationToken cancellationToken)
    {
        var demoBranchNames = Branches.Select(branch => branch.Name).ToArray();

        var branches = await _dbContext.Branches
            .Where(branch => demoBranchNames.Contains(branch.Name))
            .ToDictionaryAsync(branch => branch.Name, branch => branch.Id, cancellationToken);

        foreach (var seed in Classrooms)
        {
            if (!branches.TryGetValue(seed.BranchName, out var branchId))
                continue;

            var exists = await _dbContext.Classrooms.AnyAsync(
                classroom => classroom.BranchId == branchId && classroom.Name == seed.Name,
                cancellationToken);

            if (exists)
                continue;

            await _dbContext.Classrooms.AddAsync(new Classroom
            {
                Id = Guid.NewGuid(),
                BranchId = branchId,
                Name = seed.Name,
                Capacity = seed.Capacity,
                IsActive = true
            }, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private sealed record LanguageSeed(string Name, string Code);

    private sealed record BranchSeed(
        string Name,
        string Address,
        string PublicTransportationDirections,
        string PrivateVehicleDirections,
        string PhoneNumber,
        decimal Latitude,
        decimal Longitude,
        string[] FacilityNames);

    private sealed record ClassroomSeed(string BranchName, string Name, int Capacity);
}