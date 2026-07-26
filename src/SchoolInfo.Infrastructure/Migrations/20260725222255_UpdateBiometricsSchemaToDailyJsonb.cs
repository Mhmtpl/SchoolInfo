using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolInfo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBiometricsSchemaToDailyJsonb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eski test verilerini temizleyerek şema geçişini hatasız hale getiriyoruz
            migrationBuilder.Sql("TRUNCATE TABLE \"StudentBiometricRecords\" CASCADE;");

            migrationBuilder.DropIndex(
                name: "IX_StudentBiometricRecords_StudentId",
                table: "StudentBiometricRecords");

            migrationBuilder.RenameColumn(
                name: "SpO2",
                table: "StudentBiometricRecords",
                newName: "AverageSpO2");

            migrationBuilder.RenameColumn(
                name: "HeartRate",
                table: "StudentBiometricRecords",
                newName: "AverageHeartRate");

            migrationBuilder.RenameColumn(
                name: "BodyTemperature",
                table: "StudentBiometricRecords",
                newName: "AverageBodyTemperature");

            migrationBuilder.AddColumn<string>(
                name: "DataJson",
                table: "StudentBiometricRecords",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "StudentBiometricRecords",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_StudentBiometricRecords_StudentId_Date",
                table: "StudentBiometricRecords",
                columns: new[] { "StudentId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentBiometricRecords_StudentId_Date",
                table: "StudentBiometricRecords");

            migrationBuilder.DropColumn(
                name: "DataJson",
                table: "StudentBiometricRecords");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "StudentBiometricRecords");

            migrationBuilder.RenameColumn(
                name: "AverageSpO2",
                table: "StudentBiometricRecords",
                newName: "SpO2");

            migrationBuilder.RenameColumn(
                name: "AverageHeartRate",
                table: "StudentBiometricRecords",
                newName: "HeartRate");

            migrationBuilder.RenameColumn(
                name: "AverageBodyTemperature",
                table: "StudentBiometricRecords",
                newName: "BodyTemperature");

            migrationBuilder.CreateIndex(
                name: "IX_StudentBiometricRecords_StudentId",
                table: "StudentBiometricRecords",
                column: "StudentId");
        }
    }
}
