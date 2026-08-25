namespace LanguageCourseManagement.Tests.Infrastructure;

using Xunit;

public sealed class SqlServerEnrollmentFixture
{
    public const string NotVerifiedReason = "not-verified: no explicitly configured disposable SQL Server test database was provided, and the test project does not reference Infrastructure.";

    [Fact(Skip = NotVerifiedReason)]
    public void Migration_order_and_sql_server_concurrency_are_not_verified_without_disposable_database()
    {
    }
}
