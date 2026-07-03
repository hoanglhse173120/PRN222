using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectToChatSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubjectId",
                table: "ChatSessions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_SubjectId",
                table: "ChatSessions",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatSessions_Subjects_SubjectId",
                table: "ChatSessions",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "SubjectId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatSessions_Subjects_SubjectId",
                table: "ChatSessions");

            migrationBuilder.DropIndex(
                name: "IX_ChatSessions_SubjectId",
                table: "ChatSessions");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "ChatSessions");
        }
    }
}
