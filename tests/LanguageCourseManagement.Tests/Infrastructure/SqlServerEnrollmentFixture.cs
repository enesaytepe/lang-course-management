namespace LanguageCourseManagement.Tests.Infrastructure;

public sealed class SqlServerEnrollmentFixture
{
    public const string NotVerifiedReason = "not-verified: no explicitly configured disposable SQL Server test database was provided (set TEST_CONNECTION_STRING and TEST_SQL_DISPOSABLE=true to enable real migration/concurrency verification).";

    public static string? ConnectionString =>
        IsConfigured ? Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING") : null;

    public static bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING")) &&
        string.Equals(Environment.GetEnvironmentVariable("TEST_SQL_DISPOSABLE"), "true", StringComparison.OrdinalIgnoreCase);

}
