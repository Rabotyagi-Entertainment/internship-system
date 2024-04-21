using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Internship_system.DAL.Migrations
{
    /// <inheritdoc />
    public partial class fix_practice_diary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StudentFullName",
                table: "PracticeDiaries",
                newName: "WorkName");

            migrationBuilder.RenameColumn(
                name: "CourseWorkName",
                table: "PracticeDiaries",
                newName: "StudentCharacteristics");

            migrationBuilder.AddColumn<int>(
                name: "DiaryType",
                table: "PracticeDiaries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EditedAt",
                table: "PracticeDiaries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                table: "Internships",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiaryType",
                table: "PracticeDiaries");

            migrationBuilder.DropColumn(
                name: "EditedAt",
                table: "PracticeDiaries");

            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "Internships");

            migrationBuilder.RenameColumn(
                name: "WorkName",
                table: "PracticeDiaries",
                newName: "StudentFullName");

            migrationBuilder.RenameColumn(
                name: "StudentCharacteristics",
                table: "PracticeDiaries",
                newName: "CourseWorkName");
        }
    }
}
