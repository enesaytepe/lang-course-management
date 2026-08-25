using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageCourseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FacilityNameNormalizedUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS
                (
                    SELECT 1
                    FROM [Facilities]
                    WHERE [IsDeleted] = 0
                      AND [Name] IS NOT NULL
                    GROUP BY LOWER(LTRIM(RTRIM([Name]))) COLLATE Latin1_General_100_CI_AS
                    HAVING COUNT(*) > 1
                )
                BEGIN
                    THROW 51000, 'FacilityNameNormalizedUniqueness migration blocked: non-deleted Facilities contain duplicate names after trimming and case normalization.', 1;
                END;
                """);

            migrationBuilder.DropIndex(
                name: "UX_Facilities_Name_Active",
                table: "Facilities");

            migrationBuilder.Sql(
                """
                UPDATE [Facilities]
                SET [Name] = LTRIM(RTRIM([Name]))
                WHERE [IsDeleted] = 0
                  AND [Name] IS NOT NULL
                  AND DATALENGTH([Name]) <> DATALENGTH(LTRIM(RTRIM([Name])));
                """);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Facilities",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                computedColumnSql: "LOWER(LTRIM(RTRIM([Name])))",
                stored: true,
                collation: "Latin1_General_100_CI_AS");

            migrationBuilder.CreateIndex(
                name: "UX_Facilities_Name_Active",
                table: "Facilities",
                column: "NormalizedName",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Facilities_Name_Active",
                table: "Facilities");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Facilities");

            migrationBuilder.CreateIndex(
                name: "UX_Facilities_Name_Active",
                table: "Facilities",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");
        }
    }
}
