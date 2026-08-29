using System.Net;
using System.Net.Http.Json;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Application.DTOs.Enrollments;
using LanguageCourseManagement.Domain.Enums;
using Moq;
using Xunit;

namespace LanguageCourseManagement.Tests.Mvc;

public sealed class EnrollmentApiIntegrationTests : IClassFixture<MvcTestFactory>
{
    private readonly MvcTestFactory _factory;

    public EnrollmentApiIntegrationTests(MvcTestFactory factory) => _factory = factory;

    [Fact]
    public async Task Create_without_authentication_returns_401()
    {
        using var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/enrollments", Request());
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_without_required_role_returns_403()
    {
        using var client = AuthenticatedClient("OtherRole");
        var response = await client.PostAsJsonAsync("/api/enrollments", Request());
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_without_antiforgery_token_returns_400()
    {
        using var client = AuthenticatedClient("RegistrationOfficer");
        var response = await client.PostAsJsonAsync("/api/enrollments", Request());
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Idempotency_conflict_returns_409_problem_response()
    {
        _factory.PaymentService
            .Setup(service => service.EnrollWithPaymentAsync(
                It.IsAny<LanguageCourseManagement.Application.DTOs.Enrollments.EnrollmentCreateRequest>(),
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessException("İdempotensi anahtarı farklı bir tahsilatla zaten ilişkilendirilmiş."));

        using var client = AuthenticatedClient("RegistrationOfficer");
        var tokenResponse = await client.GetAsync("/__test/antiforgery");
        var tokens = (Cookie: tokenResponse.Headers.GetValues("Set-Cookie").First().Split(';')[0],
            RequestToken: tokenResponse.Headers.GetValues("X-Test-Antiforgery").Single());
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/enrollments")
        {
            Content = JsonContent.Create(Request())
        };
        request.Headers.Add("Cookie", tokens.Cookie);
        request.Headers.Add("X-XSRF-TOKEN", tokens.RequestToken);

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.NotEmpty(await response.Content.ReadAsStringAsync());
    }

    private HttpClient AuthenticatedClient(string role)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-User", Guid.NewGuid().ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", role);
        return client;
    }

    private static EnrollmentCreateApiPayload Request() => new(
        Guid.NewGuid(), Guid.NewGuid(), 0m, "integration-idempotency-key", PaymentType.Cash);

    private sealed record EnrollmentCreateApiPayload(
        Guid StudentId, Guid CourseId, decimal DiscountAmount, string IdempotencyKey, PaymentType PaymentType);
}
