using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageCourseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnrollmentSettlementIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "Payments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE [Payments] SET [IdempotencyKey] = CONVERT(nvarchar(100), [Id]) WHERE [IdempotencyKey] = N'';");

            migrationBuilder.CreateIndex(
                name: "UX_Payments_IdempotencyKey",
                table: "Payments",
                column: "IdempotencyKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Payments_IdempotencyKey",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "Payments");
        }
    }
}
