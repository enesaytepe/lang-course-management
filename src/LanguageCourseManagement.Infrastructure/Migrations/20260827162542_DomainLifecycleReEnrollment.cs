using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageCourseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DomainLifecycleReEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Phase 5 — Data Migration: Convert soft-deleted master records to IsActive=false, IsDeleted=false
            migrationBuilder.Sql("UPDATE Branches SET IsActive = 0, IsDeleted = 0, DeletedAt = NULL WHERE IsDeleted = 1");
            migrationBuilder.Sql("UPDATE Teachers SET IsActive = 0, IsDeleted = 0, DeletedAt = NULL WHERE IsDeleted = 1");
            migrationBuilder.Sql("UPDATE Classrooms SET IsActive = 0, IsDeleted = 0, DeletedAt = NULL WHERE IsDeleted = 1");
            migrationBuilder.Sql("UPDATE Enrollments SET PaymentType = 1 WHERE PaymentType = 0");

            migrationBuilder.DropIndex(
                name: "UX_Payments_Enrollment",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Payments_Method_Cash",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Payments_Status_Settled",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "UX_Enrollments_Student_Course",
                table: "Enrollments");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Payments",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InstallmentId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Payments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "datetime2(0)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Enrollments",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Enrollments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PaymentType",
                table: "Enrollments",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Courses",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Courses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Installments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstallmentNumber = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Installments", x => x.Id);
                    table.CheckConstraint("CK_Installments_Amount_NonNegative", "[Amount] >= 0");
                    table.CheckConstraint("CK_Installments_InstallmentNumber_Positive", "[InstallmentNumber] > 0");
                    table.CheckConstraint("CK_Installments_Status_Range", "[Status] BETWEEN 1 AND 4");
                    table.ForeignKey(
                        name: "FK_Installments_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Enrollment",
                table: "Payments",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InstallmentId",
                table: "Payments",
                column: "InstallmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status_IsDeleted_EnrollmentId",
                table: "Payments",
                columns: new[] { "Status", "IsDeleted", "EnrollmentId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Payments_Method_Range",
                table: "Payments",
                sql: "[Method] BETWEEN 1 AND 3");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Payments_Status_Range",
                table: "Payments",
                sql: "[Status] BETWEEN 1 AND 4");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_Status_CourseId",
                table: "Enrollments",
                columns: new[] { "Status", "CourseId" });

            migrationBuilder.CreateIndex(
                name: "UX_Enrollments_Student_Course",
                table: "Enrollments",
                columns: new[] { "StudentId", "CourseId" },
                unique: true,
                filter: "[Status] != 3");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Enrollments_PaymentType_Range",
                table: "Enrollments",
                sql: "[PaymentType] BETWEEN 1 AND 2");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Branches_Latitude_Range",
                table: "Branches",
                sql: "[Latitude] >= -90 AND [Latitude] <= 90");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Branches_Longitude_Range",
                table: "Branches",
                sql: "[Longitude] >= -180 AND [Longitude] <= 180");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityName",
                table: "AuditLogs",
                column: "EntityName");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UX_Installments_Enrollment_Number",
                table: "Installments",
                columns: new[] { "EnrollmentId", "InstallmentNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Installments_InstallmentId",
                table: "Payments",
                column: "InstallmentId",
                principalTable: "Installments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Installments_InstallmentId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Installments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_Enrollment",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_InstallmentId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_Status_IsDeleted_EnrollmentId",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Payments_Method_Range",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Payments_Status_Range",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_Status_CourseId",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "UX_Enrollments_Student_Course",
                table: "Enrollments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Enrollments_PaymentType_Range",
                table: "Enrollments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Branches_Latitude_Range",
                table: "Branches");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Branches_Longitude_Range",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "InstallmentId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "UX_Payments_Enrollment",
                table: "Payments",
                column: "EnrollmentId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Payments_Method_Cash",
                table: "Payments",
                sql: "[Method] = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Payments_Status_Settled",
                table: "Payments",
                sql: "[Status] = 1");

            migrationBuilder.CreateIndex(
                name: "UX_Enrollments_Student_Course",
                table: "Enrollments",
                columns: new[] { "StudentId", "CourseId" },
                unique: true);
        }
    }
}
