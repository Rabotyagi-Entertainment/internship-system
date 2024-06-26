using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Internship_system.DAL.Migrations
{
    /// <inheritdoc />
    public partial class add_cascade_delete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_InternshipProgresses_InternshipProgressId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_PracticeDiaries_PracticeDiaryId",
                table: "Comments");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_InternshipProgresses_InternshipProgressId",
                table: "Comments",
                column: "InternshipProgressId",
                principalTable: "InternshipProgresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_PracticeDiaries_PracticeDiaryId",
                table: "Comments",
                column: "PracticeDiaryId",
                principalTable: "PracticeDiaries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
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
    }
}
