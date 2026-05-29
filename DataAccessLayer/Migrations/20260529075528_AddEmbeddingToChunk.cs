using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddEmbeddingToChunk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatSessions",
                columns: table => new
                {
                    SessionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatSessions", x => x.SessionID);
                });

            migrationBuilder.CreateTable(
                name: "ExperimentConfigs",
                columns: table => new
                {
                    ConfigID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApproachType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmbeddingModel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChunkingStrategy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChunkSize = table.Column<int>(type: "int", nullable: true),
                    ChunkOverlap = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperimentConfigs", x => x.ConfigID);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    SubjectID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.SubjectID);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    MessageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionID = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MessageText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.MessageID);
                    table.ForeignKey(
                        name: "FK_ChatMessages_ChatSessions_SessionID",
                        column: x => x.SessionID,
                        principalTable: "ChatSessions",
                        principalColumn: "SessionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    DocumentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSizeKB = table.Column<long>(type: "bigint", nullable: true),
                    IsIndexed = table.Column<bool>(type: "bit", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.DocumentID);
                    table.ForeignKey(
                        name: "FK_Documents_Subjects_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestQuestions",
                columns: table => new
                {
                    QuestionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectID = table.Column<int>(type: "int", nullable: true),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroundTruth = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestQuestions", x => x.QuestionID);
                    table.ForeignKey(
                        name: "FK_TestQuestions_Subjects_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DocumentChunks",
                columns: table => new
                {
                    ChunkID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentID = table.Column<int>(type: "int", nullable: false),
                    ChunkContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChunkIndex = table.Column<int>(type: "int", nullable: true),
                    PageNumber = table.Column<int>(type: "int", nullable: true),
                    Embedding = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentChunks", x => x.ChunkID);
                    table.ForeignKey(
                        name: "FK_DocumentChunks_Documents_DocumentID",
                        column: x => x.DocumentID,
                        principalTable: "Documents",
                        principalColumn: "DocumentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BenchmarkResults",
                columns: table => new
                {
                    ResultID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigID = table.Column<int>(type: "int", nullable: false),
                    QuestionID = table.Column<int>(type: "int", nullable: false),
                    ModelResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Faithfulness = table.Column<double>(type: "float", nullable: true),
                    AnswerRelevance = table.Column<double>(type: "float", nullable: true),
                    ContextPrecision = table.Column<double>(type: "float", nullable: true),
                    ContextRecall = table.Column<double>(type: "float", nullable: true),
                    EvaluatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenchmarkResults", x => x.ResultID);
                    table.ForeignKey(
                        name: "FK_BenchmarkResults_ExperimentConfigs_ConfigID",
                        column: x => x.ConfigID,
                        principalTable: "ExperimentConfigs",
                        principalColumn: "ConfigID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BenchmarkResults_TestQuestions_QuestionID",
                        column: x => x.QuestionID,
                        principalTable: "TestQuestions",
                        principalColumn: "QuestionID");
                });

            migrationBuilder.CreateTable(
                name: "MessageSources",
                columns: table => new
                {
                    SourceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageID = table.Column<int>(type: "int", nullable: false),
                    ChunkID = table.Column<int>(type: "int", nullable: false),
                    RelevanceScore = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageSources", x => x.SourceID);
                    table.ForeignKey(
                        name: "FK_MessageSources_ChatMessages_MessageID",
                        column: x => x.MessageID,
                        principalTable: "ChatMessages",
                        principalColumn: "MessageID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageSources_DocumentChunks_ChunkID",
                        column: x => x.ChunkID,
                        principalTable: "DocumentChunks",
                        principalColumn: "ChunkID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BenchmarkResults_ConfigID",
                table: "BenchmarkResults",
                column: "ConfigID");

            migrationBuilder.CreateIndex(
                name: "IX_BenchmarkResults_QuestionID",
                table: "BenchmarkResults",
                column: "QuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SessionID",
                table: "ChatMessages",
                column: "SessionID");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentChunks_DocumentID",
                table: "DocumentChunks",
                column: "DocumentID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_SubjectID",
                table: "Documents",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_MessageSources_ChunkID",
                table: "MessageSources",
                column: "ChunkID");

            migrationBuilder.CreateIndex(
                name: "IX_MessageSources_MessageID",
                table: "MessageSources",
                column: "MessageID");

            migrationBuilder.CreateIndex(
                name: "IX_TestQuestions_SubjectID",
                table: "TestQuestions",
                column: "SubjectID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BenchmarkResults");

            migrationBuilder.DropTable(
                name: "MessageSources");

            migrationBuilder.DropTable(
                name: "ExperimentConfigs");

            migrationBuilder.DropTable(
                name: "TestQuestions");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "DocumentChunks");

            migrationBuilder.DropTable(
                name: "ChatSessions");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Subjects");
        }
    }
}
