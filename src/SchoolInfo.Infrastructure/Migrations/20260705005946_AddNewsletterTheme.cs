using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolInfo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsletterTheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Theme",
                table: "Newsletters",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Theme",
                table: "Newsletters");
        }
    }
}
