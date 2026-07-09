using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Avility.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLogoStorageKeyColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoStorageKey",
                table: "Companies",
                type: "TEXT",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoStorageKey",
                table: "Companies");
        }
    }
}
