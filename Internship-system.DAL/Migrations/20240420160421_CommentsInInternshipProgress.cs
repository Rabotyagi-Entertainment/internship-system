using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Internship_system.DAL.Migrations
{
    /// <inheritdoc />
    public partial class CommentsInInternshipProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_PracticeDiaries_PracticeDiaryId",
                table: "Comments");

            migrationBuilder.AlterColumn<Guid>(
                name: "PracticeDiaryId",
                table: "Comments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "InternshipProgressId",
                table: "Comments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_InternshipProgressId",
                table: "Comments",
                column: "InternshipProgressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_InternshipProgresses_InternshipProgressId",
                table: "Comments",
                column: "InternshipProgressId",
                principalTable: "InternshipProgresses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_PracticeDiaries_PracticeDiaryId",
                table: "Comments",
                column: "PracticeDiaryId",
                principalTable: "PracticeDiaries",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_InternshipProgresses_InternshipProgressId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_PracticeDiaries_PracticeDiaryId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_InternshipProgressId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "InternshipProgressId",
                table: "Comments");

            migrationBuilder.AlterColumn<Guid>(
                name: "PracticeDiaryId",
                table: "Comments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_PracticeDiaries_PracticeDiaryId",
                table: "Comments",
                column: "PracticeDiaryId",
                principalTable: "PracticeDiaries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
