using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolInfo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MultipleTeachersPerClassroom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "Classrooms");

            migrationBuilder.CreateTable(
                name: "ClassroomTeachers",
                columns: table => new
                {
                    ClassroomsId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeachersId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassroomTeachers", x => new { x.ClassroomsId, x.TeachersId });
                    table.ForeignKey(
                        name: "FK_ClassroomTeachers_Classrooms_ClassroomsId",
                        column: x => x.ClassroomsId,
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassroomTeachers_Users_TeachersId",
                        column: x => x.TeachersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassroomTeachers_TeachersId",
                table: "ClassroomTeachers",
                column: "TeachersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassroomTeachers");

            migrationBuilder.AddColumn<Guid>(
                name: "TeacherId",
                table: "Classrooms",
                type: "uuid",
                nullable: true);
        }
    }
}
