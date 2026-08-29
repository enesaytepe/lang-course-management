using System.Security.Claims;
using System.Text.Encodings.Web;
using LanguageCourseManagement.Application.Services.PaymentService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace LanguageCourseManagement.Tests.Mvc;

public sealed class MvcTestFactory : Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>
{
    public Mock<IPaymentService> PaymentService { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Server=(local);Database=unused;Trusted_Connection=True;");
        builder.UseSetting("Database:RunInitialization", "false");
        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthenticationHandler.TestScheme;
                    options.DefaultChallengeScheme = TestAuthenticationHandler.TestScheme;
                    options.DefaultScheme = TestAuthenticationHandler.TestScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.TestScheme, _ => { });

            var paymentDescriptor = services.Single(service => service.ServiceType == typeof(IPaymentService));
            services.Remove(paymentDescriptor);
            services.AddSingleton(PaymentService.Object);
            services.AddTransient<Microsoft.AspNetCore.Hosting.IStartupFilter, AntiforgeryStartupFilter>();
        });
    }

}

internal sealed class AntiforgeryStartupFilter : Microsoft.AspNetCore.Hosting.IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        app.Use(async (context, continuation) =>
        {
            if (context.Request.Path == "/__test/antiforgery")
            {
                var authentication = context.RequestServices.GetRequiredService<IAuthenticationService>();
                var authenticationResult = await authentication.AuthenticateAsync(context, TestAuthenticationHandler.TestScheme);
                if (authenticationResult.Principal is not null)
                    context.User = authenticationResult.Principal;

                var tokens = context.RequestServices
                    .GetRequiredService<Microsoft.AspNetCore.Antiforgery.IAntiforgery>()
                    .GetAndStoreTokens(context);
                context.Response.Headers.Append("X-Test-Antiforgery", tokens.RequestToken!);
                return;
            }

            await continuation();
        });
        next(app);
    };
}

public sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string TestScheme = "IntegrationTest";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Test-User", out var user) || string.IsNullOrWhiteSpace(user))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.ToString())
        };
        if (Request.Headers.TryGetValue("X-Test-Role", out var role) && !string.IsNullOrWhiteSpace(role))
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, TestScheme));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, TestScheme)));
    }
}
