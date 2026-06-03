using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntityIdCasingAndNullability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BenchmarkResults_ExperimentConfigs_ConfigID",
                table: "BenchmarkResults");

            migrationBuilder.DropForeignKey(
                name: "FK_BenchmarkResults_TestQuestions_QuestionID",
                table: "BenchmarkResults");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_ChatSessions_SessionID",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentChunks_Documents_DocumentID",
                table: "DocumentChunks");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Subjects_SubjectID",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageSources_ChatMessages_MessageID",
                table: "MessageSources");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageSources_DocumentChunks_ChunkID",
                table: "MessageSources");

            migrationBuilder.DropForeignKey(
                name: "FK_TestQuestions_Subjects_SubjectID",
                table: "TestQuestions");

            migrationBuilder.RenameColumn(
                name: "SubjectID",
                table: "TestQuestions",
                newName: "SubjectId");

            migrationBuilder.RenameColumn(
                name: "QuestionID",
                table: "TestQuestions",
                newName: "QuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_TestQuestions_SubjectID",
                table: "TestQuestions",
                newName: "IX_TestQuestions_SubjectId");

            migrationBuilder.RenameColumn(
                name: "SubjectID",
                table: "Subjects",
                newName: "SubjectId");

            migrationBuilder.RenameColumn(
                name: "MessageID",
                table: "MessageSources",
                newName: "MessageId");

            migrationBuilder.RenameColumn(
                name: "ChunkID",
                table: "MessageSources",
                newName: "ChunkId");

            migrationBuilder.RenameColumn(
                name: "SourceID",
                table: "MessageSources",
                newName: "SourceId");

            migrationBuilder.RenameIndex(
                name: "IX_MessageSources_MessageID",
                table: "MessageSources",
                newName: "IX_MessageSources_MessageId");

            migrationBuilder.RenameIndex(
                name: "IX_MessageSources_ChunkID",
                table: "MessageSources",
                newName: "IX_MessageSources_ChunkId");

            migrationBuilder.RenameColumn(
                name: "ConfigID",
                table: "ExperimentConfigs",
                newName: "ConfigId");

            migrationBuilder.RenameColumn(
                name: "SubjectID",
                table: "Documents",
                newName: "SubjectId");

            migrationBuilder.RenameColumn(
                name: "FileSizeKB",
                table: "Documents",
                newName: "FileSizeKb");

            migrationBuilder.RenameColumn(
                name: "DocumentID",
                table: "Documents",
                newName: "DocumentId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_SubjectID",
                table: "Documents",
                newName: "IX_Documents_SubjectId");

            migrationBuilder.RenameColumn(
                name: "DocumentID",
                table: "DocumentChunks",
                newName: "DocumentId");

            migrationBuilder.RenameColumn(
                name: "ChunkID",
                table: "DocumentChunks",
                newName: "ChunkId");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentChunks_DocumentID",
                table: "DocumentChunks",
                newName: "IX_DocumentChunks_DocumentId");

            migrationBuilder.RenameColumn(
                name: "SessionID",
                table: "ChatSessions",
                newName: "SessionId");

            migrationBuilder.RenameColumn(
                name: "SessionID",
                table: "ChatMessages",
                newName: "SessionId");

            migrationBuilder.RenameColumn(
                name: "MessageID",
                table: "ChatMessages",
                newName: "MessageId");

            migrationBuilder.RenameIndex(
                name: "IX_ChatMessages_SessionID",
                table: "ChatMessages",
                newName: "IX_ChatMessages_SessionId");

            migrationBuilder.RenameColumn(
                name: "QuestionID",
                table: "BenchmarkResults",
                newName: "QuestionId");

            migrationBuilder.RenameColumn(
                name: "ConfigID",
                table: "BenchmarkResults",
                newName: "ConfigId");

            migrationBuilder.RenameColumn(
                name: "ResultID",
                table: "BenchmarkResults",
                newName: "ResultId");

            migrationBuilder.RenameIndex(
                name: "IX_BenchmarkResults_QuestionID",
                table: "BenchmarkResults",
                newName: "IX_BenchmarkResults_QuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_BenchmarkResults_ConfigID",
                table: "BenchmarkResults",
                newName: "IX_BenchmarkResults_ConfigId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TestQuestions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Subjects",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ExperimentConfigs",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UploadedAt",
                table: "Documents",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "IsIndexed",
                table: "Documents",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "DocumentChunks",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "SessionName",
                table: "ChatSessions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ChatSessions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Timestamp",
                table: "ChatMessages",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EvaluatedAt",
                table: "BenchmarkResults",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddForeignKey(
                name: "FK_BenchmarkResults_ExperimentConfigs_ConfigId",
                table: "BenchmarkResults",
                column: "ConfigId",
                principalTable: "ExperimentConfigs",
                principalColumn: "ConfigId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BenchmarkResults_TestQuestions_QuestionId",
                table: "BenchmarkResults",
                column: "QuestionId",
                principalTable: "TestQuestions",
                principalColumn: "QuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_ChatSessions_SessionId",
                table: "ChatMessages",
                column: "SessionId",
                principalTable: "ChatSessions",
                principalColumn: "SessionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentChunks_Documents_DocumentId",
                table: "DocumentChunks",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "DocumentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Subjects_SubjectId",
                table: "Documents",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "SubjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageSources_ChatMessages_MessageId",
                table: "MessageSources",
                column: "MessageId",
                principalTable: "ChatMessages",
                principalColumn: "MessageId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageSources_DocumentChunks_ChunkId",
                table: "MessageSources",
                column: "ChunkId",
                principalTable: "DocumentChunks",
                principalColumn: "ChunkId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestQuestions_Subjects_SubjectId",
                table: "TestQuestions",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "SubjectId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BenchmarkResults_ExperimentConfigs_ConfigId",
                table: "BenchmarkResults");

            migrationBuilder.DropForeignKey(
                name: "FK_BenchmarkResults_TestQuestions_QuestionId",
                table: "BenchmarkResults");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_ChatSessions_SessionId",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentChunks_Documents_DocumentId",
                table: "DocumentChunks");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Subjects_SubjectId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageSources_ChatMessages_MessageId",
                table: "MessageSources");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageSources_DocumentChunks_ChunkId",
                table: "MessageSources");

            migrationBuilder.DropForeignKey(
                name: "FK_TestQuestions_Subjects_SubjectId",
                table: "TestQuestions");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "TestQuestions",
                newName: "SubjectID");

            migrationBuilder.RenameColumn(
                name: "QuestionId",
                table: "TestQuestions",
                newName: "QuestionID");

            migrationBuilder.RenameIndex(
                name: "IX_TestQuestions_SubjectId",
                table: "TestQuestions",
                newName: "IX_TestQuestions_SubjectID");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "Subjects",
                newName: "SubjectID");

            migrationBuilder.RenameColumn(
                name: "MessageId",
                table: "MessageSources",
                newName: "MessageID");

            migrationBuilder.RenameColumn(
                name: "ChunkId",
                table: "MessageSources",
                newName: "ChunkID");

            migrationBuilder.RenameColumn(
                name: "SourceId",
                table: "MessageSources",
                newName: "SourceID");

            migrationBuilder.RenameIndex(
                name: "IX_MessageSources_MessageId",
                table: "MessageSources",
                newName: "IX_MessageSources_MessageID");

            migrationBuilder.RenameIndex(
                name: "IX_MessageSources_ChunkId",
                table: "MessageSources",
                newName: "IX_MessageSources_ChunkID");

            migrationBuilder.RenameColumn(
                name: "ConfigId",
                table: "ExperimentConfigs",
                newName: "ConfigID");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "Documents",
                newName: "SubjectID");

            migrationBuilder.RenameColumn(
                name: "FileSizeKb",
                table: "Documents",
                newName: "FileSizeKB");

            migrationBuilder.RenameColumn(
                name: "DocumentId",
                table: "Documents",
                newName: "DocumentID");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_SubjectId",
                table: "Documents",
                newName: "IX_Documents_SubjectID");

            migrationBuilder.RenameColumn(
                name: "DocumentId",
                table: "DocumentChunks",
                newName: "DocumentID");

            migrationBuilder.RenameColumn(
                name: "ChunkId",
                table: "DocumentChunks",
                newName: "ChunkID");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentChunks_DocumentId",
                table: "DocumentChunks",
                newName: "IX_DocumentChunks_DocumentID");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "ChatSessions",
                newName: "SessionID");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "ChatMessages",
                newName: "SessionID");

            migrationBuilder.RenameColumn(
                name: "MessageId",
                table: "ChatMessages",
                newName: "MessageID");

            migrationBuilder.RenameIndex(
                name: "IX_ChatMessages_SessionId",
                table: "ChatMessages",
                newName: "IX_ChatMessages_SessionID");

            migrationBuilder.RenameColumn(
                name: "QuestionId",
                table: "BenchmarkResults",
                newName: "QuestionID");

            migrationBuilder.RenameColumn(
                name: "ConfigId",
                table: "BenchmarkResults",
                newName: "ConfigID");

            migrationBuilder.RenameColumn(
                name: "ResultId",
                table: "BenchmarkResults",
                newName: "ResultID");

            migrationBuilder.RenameIndex(
                name: "IX_BenchmarkResults_QuestionId",
                table: "BenchmarkResults",
                newName: "IX_BenchmarkResults_QuestionID");

            migrationBuilder.RenameIndex(
                name: "IX_BenchmarkResults_ConfigId",
                table: "BenchmarkResults",
                newName: "IX_BenchmarkResults_ConfigID");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TestQuestions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Subjects",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ExperimentConfigs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UploadedAt",
                table: "Documents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsIndexed",
                table: "Documents",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "DocumentChunks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SessionName",
                table: "ChatSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ChatSessions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Timestamp",
                table: "ChatMessages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EvaluatedAt",
                table: "BenchmarkResults",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BenchmarkResults_ExperimentConfigs_ConfigID",
                table: "BenchmarkResults",
                column: "ConfigID",
                principalTable: "ExperimentConfigs",
                principalColumn: "ConfigID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BenchmarkResults_TestQuestions_QuestionID",
                table: "BenchmarkResults",
                column: "QuestionID",
                principalTable: "TestQuestions",
                principalColumn: "QuestionID");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_ChatSessions_SessionID",
                table: "ChatMessages",
                column: "SessionID",
                principalTable: "ChatSessions",
                principalColumn: "SessionID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentChunks_Documents_DocumentID",
                table: "DocumentChunks",
                column: "DocumentID",
                principalTable: "Documents",
                principalColumn: "DocumentID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Subjects_SubjectID",
                table: "Documents",
                column: "SubjectID",
                principalTable: "Subjects",
                principalColumn: "SubjectID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageSources_ChatMessages_MessageID",
                table: "MessageSources",
                column: "MessageID",
                principalTable: "ChatMessages",
                principalColumn: "MessageID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageSources_DocumentChunks_ChunkID",
                table: "MessageSources",
                column: "ChunkID",
                principalTable: "DocumentChunks",
                principalColumn: "ChunkID");

            migrationBuilder.AddForeignKey(
                name: "FK_TestQuestions_Subjects_SubjectID",
                table: "TestQuestions",
                column: "SubjectID",
                principalTable: "Subjects",
                principalColumn: "SubjectID",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
