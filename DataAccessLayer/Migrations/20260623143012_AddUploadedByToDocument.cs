using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadedByToDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UploadedByUserId",
                table: "Documents",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UploadedByUserId",
                table: "Documents",
                column: "UploadedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_AspNetUsers_UploadedByUserId",
                table: "Documents",
                column: "UploadedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_AspNetUsers_UploadedByUserId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_UploadedByUserId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "UploadedByUserId",
                table: "Documents");
        }
    }
}
