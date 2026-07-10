using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Avility.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessibilityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccommodationNotes",
                table: "JobSeekers",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisabilityCategories",
                table: "JobSeekers",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AccommodationDetails",
                table: "JobPostings",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupportedDisabilityCategories",
                table: "JobPostings",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccommodationNotes",
                table: "JobSeekers");

            migrationBuilder.DropColumn(
                name: "DisabilityCategories",
                table: "JobSeekers");

            migrationBuilder.DropColumn(
                name: "AccommodationDetails",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "SupportedDisabilityCategories",
                table: "JobPostings");
        }
    }
}
