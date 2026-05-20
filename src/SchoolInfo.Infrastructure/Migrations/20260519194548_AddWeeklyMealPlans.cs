using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolInfo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWeeklyMealPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeeklyMealPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassroomId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    MealName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PlannedCalories = table.Column<int>(type: "integer", nullable: false),
                    FoodContent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ProteinGrams = table.Column<double>(type: "double precision", nullable: false),
                    CarbsGrams = table.Column<double>(type: "double precision", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyMealPlans", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyMealPlans_ClassroomId_DayOfWeek",
                table: "WeeklyMealPlans",
                columns: new[] { "ClassroomId", "DayOfWeek" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeeklyMealPlans");
        }
    }
}
