using LanguageCourseManagement.Application;
using LanguageCourseManagement.Infrastructure;
using LanguageCourseManagement.Infrastructure.Identity;
using LanguageCourseManagement.Infrastructure.Logging;
using LanguageCourseManagement.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Serilog;

try
{
    // Configure the bootstrap logger before creating the host so builder and startup failures are captured too.
    SerilogConfigurationHelper.Configure(new ConfigurationBuilder().Build());

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddControllersWithViews();
    builder.Services.AddAntiforgery(options =>
    {
        options.HeaderName = "X-XSRF-TOKEN";
        options.Cookie.Name = "LanguageCourseManagement.Antiforgery";
    });

    builder.Services.AddApplication();
    builder.Services.AddAutoMapper(_ => { }, typeof(LanguageCourseManagement.MVC.Mapping.ViewModelProfile).Assembly);
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        options.AddPolicy("SystemAdmin", policy => policy.RequireRole("SystemAdmin"));
        options.AddPolicy("RegistrationOfficer", policy => policy.RequireRole("RegistrationOfficer"));
    });

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Events.OnRedirectToLogin = async context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Title = "Authentication required",
                    Detail = "Kimlik doğrulama gerekli.",
                    Status = StatusCodes.Status401Unauthorized,
                    Type = "https://api.languagemanagement.edu.tr/problems/authentication"
                });
                return;
            }

            context.Response.Redirect(context.RedirectUri);
        };
        options.Events.OnRedirectToAccessDenied = async context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Title = "Access denied",
                    Detail = "Bu işlem için yetkiniz yok.",
                    Status = StatusCodes.Status403Forbidden,
                    Type = "https://api.languagemanagement.edu.tr/problems/authorization"
                });
                return;
            }

            context.Response.Redirect(context.RedirectUri);
        };
    });

    var app = builder.Build();

    if (builder.Configuration.GetValue<bool>("Database:RunInitialization"))
    {
        using var initializationScope = app.Services.CreateScope();
        var services = initializationScope.ServiceProvider;

        var dbContext = services.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();

        await services.GetRequiredService<IdentitySeedService>().SeedAsync();
        await services.GetRequiredService<ApplicationDataSeedService>().SeedAsync();
    }

    // Configure the HTTP request pipeline.
    app.UseMiddleware<LanguageCourseManagement.MVC.Infrastructure.Middleware.ExceptionMiddleware>();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapStaticAssets();

    app.MapControllers();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Dashboard}/{action=Index}/{id?}")
        .WithStaticAssets();

    await app.RunAsync();
}
catch (Exception exception)
{
    Log.Fatal(exception, "Host terminated unexpectedly");
    Environment.ExitCode = 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program
{
}
