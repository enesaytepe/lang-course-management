namespace LanguageCourseManagement.Tests;

using Xunit;

public sealed class EnrollmentApiAuthorizationTests
{
    [Fact(Skip = "not-verified: the test project does not reference the MVC host; controller authorization, antiforgery, and status mapping require an MVC test host and are intentionally not faked.")]
    public void Enrollment_create_requires_role_and_antiforgery()
    {
    }

    [Fact(Skip = "not-verified: the test project does not reference the MVC host; HTTP 409 replay/conflict mapping requires an MVC integration host.")]
    public void Idempotency_conflict_maps_to_http_409()
    {
    }
}
