using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace LanguageCourseManagement.Infrastructure.Identity;

public sealed class IdentitySeedService
{
    public const string SystemAdminRole = "SystemAdmin";
    public const string RegistrationOfficerRole = "RegistrationOfficer";

    private static readonly string[] Roles = [SystemAdminRole, RegistrationOfficerRole];

    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public IdentitySeedService(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _configuration = configuration;
        _environment = environment;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await EnsureRolesAsync(cancellationToken);

        if (!_environment.IsDevelopment() ||
            !_configuration.GetValue<bool>("Authentication:SeedDemoUsers"))
        {
            return;
        }

        await EnsureDemoUserAsync(
            "Authentication:DemoUsers:SystemAdmin",
            SystemAdminRole,
            cancellationToken);

        await EnsureDemoUserAsync(
            "Authentication:DemoUsers:RegistrationOfficer",
            RegistrationOfficerRole,
            cancellationToken);
    }

    private async Task EnsureRolesAsync(CancellationToken cancellationToken)
    {
        foreach (var role in Roles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await _roleManager.RoleExistsAsync(role))
                continue;

            var result = await _roleManager.CreateAsync(new IdentityRole(role));
            EnsureSuccess(result, $"role '{role}'");
        }
    }

    private async Task EnsureDemoUserAsync(
        string configurationKey,
        string role,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var userName = _configuration[$"{configurationKey}:UserName"];
        var password = _configuration[$"{configurationKey}:Password"];

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                $"Development demo user configuration is incomplete for '{role}'.");
        }

        var user = await _userManager.FindByNameAsync(userName);
        if (user is null)
        {
            user = new ApplicationUser { UserName = userName };
            var createResult = await _userManager.CreateAsync(user, password);
            EnsureSuccess(createResult, $"demo user '{userName}'");
        }

        if (await _userManager.IsInRoleAsync(user, role))
            return;

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        EnsureSuccess(roleResult, $"demo user role '{role}'");
    }

    private static void EnsureSuccess(IdentityResult result, string operation)
    {
        if (result.Succeeded)
            return;

        throw new InvalidOperationException(
            $"Identity seed failed while creating {operation}: " +
            string.Join(", ", result.Errors.Select(error => error.Code)));
    }
}