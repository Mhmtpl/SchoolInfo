using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolInfo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNutritionToMealRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CarbsGrams",
                table: "MealRecords",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FoodContent",
                table: "MealRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlannedCalories",
                table: "MealRecords",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ProteinGrams",
                table: "MealRecords",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarbsGrams",
                table: "MealRecords");

            migrationBuilder.DropColumn(
                name: "FoodContent",
                table: "MealRecords");

            migrationBuilder.DropColumn(
                name: "PlannedCalories",
                table: "MealRecords");

            migrationBuilder.DropColumn(
                name: "ProteinGrams",
                table: "MealRecords");
        }
    }
}
