using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageCourseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TeacherCourseLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeacherCourseLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherCourseLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherCourseLevels_CourseLevels_CourseLevelId",
                        column: x => x.CourseLevelId,
                        principalTable: "CourseLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherCourseLevels_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherCourseLevels_CourseLevelId",
                table: "TeacherCourseLevels",
                column: "CourseLevelId");

            migrationBuilder.CreateIndex(
                name: "UX_TeacherCourseLevels_Teacher_CourseLevel",
                table: "TeacherCourseLevels",
                columns: new[] { "TeacherId", "CourseLevelId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeacherCourseLevels");
        }
    }
}
