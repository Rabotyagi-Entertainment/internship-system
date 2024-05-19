using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Internship_system.DAL.Migrations
{
    /// <inheritdoc />
    public partial class fix_internship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "Internships");

            migrationBuilder.AddColumn<DateTime>(
                name: "OrderDate",
                table: "PracticeDiaries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                table: "PracticeDiaries",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderDate",
                table: "PracticeDiaries");

            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "PracticeDiaries");

            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                table: "Internships",
                type: "text",
                nullable: true);
        }
    }
}
