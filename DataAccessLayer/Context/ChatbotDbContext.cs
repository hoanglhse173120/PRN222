using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Context;

public class ChatbotDbContext : DbContext
{
    public ChatbotDbContext(DbContextOptions<ChatbotDbContext> options) : base(options) { }

    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentChunk> DocumentChunks { get; set; }
    public DbSet<ChatSession> ChatSessions { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<MessageSource> MessageSources { get; set; }
    public DbSet<ExperimentConfig> ExperimentConfigs { get; set; }
    public DbSet<TestQuestion> TestQuestions { get; set; }
    public DbSet<BenchmarkResult> BenchmarkResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Subject → Documents (1:N, cascade delete)
        modelBuilder.Entity<Document>()
            .HasOne(d => d.Subject)
            .WithMany(s => s.Documents)
            .HasForeignKey(d => d.SubjectID)
            .OnDelete(DeleteBehavior.Cascade);

        // Document → DocumentChunks (1:N, cascade delete)
        modelBuilder.Entity<DocumentChunk>()
            .HasOne(c => c.Document)
            .WithMany(d => d.DocumentChunks)
            .HasForeignKey(c => c.DocumentID)
            .OnDelete(DeleteBehavior.Cascade);

        // ChatSession → ChatMessages (1:N, cascade delete)
        modelBuilder.Entity<ChatMessage>()
            .HasOne(m => m.ChatSession)
            .WithMany(s => s.ChatMessages)
            .HasForeignKey(m => m.SessionID)
            .OnDelete(DeleteBehavior.Cascade);

        // ChatMessage → MessageSources (1:N, cascade delete)
        modelBuilder.Entity<MessageSource>()
            .HasOne(ms => ms.ChatMessage)
            .WithMany(m => m.MessageSources)
            .HasForeignKey(ms => ms.MessageID)
            .OnDelete(DeleteBehavior.Cascade);

        // DocumentChunk → MessageSources (1:N, no cascade to avoid multiple paths)
        modelBuilder.Entity<MessageSource>()
            .HasOne(ms => ms.DocumentChunk)
            .WithMany(c => c.MessageSources)
            .HasForeignKey(ms => ms.ChunkID)
            .OnDelete(DeleteBehavior.NoAction);

        // ExperimentConfig → BenchmarkResults (1:N, cascade delete)
        modelBuilder.Entity<BenchmarkResult>()
            .HasOne(b => b.ExperimentConfig)
            .WithMany(e => e.BenchmarkResults)
            .HasForeignKey(b => b.ConfigID)
            .OnDelete(DeleteBehavior.Cascade);

        // TestQuestion → BenchmarkResults (1:N, no cascade)
        modelBuilder.Entity<BenchmarkResult>()
            .HasOne(b => b.TestQuestion)
            .WithMany(q => q.BenchmarkResults)
            .HasForeignKey(b => b.QuestionID)
            .OnDelete(DeleteBehavior.NoAction);

        // Subject → TestQuestions (1:N, nullable FK)
        modelBuilder.Entity<TestQuestion>()
            .HasOne(q => q.Subject)
            .WithMany(s => s.TestQuestions)
            .HasForeignKey(q => q.SubjectID)
            .OnDelete(DeleteBehavior.SetNull);

        // Column constraints
        modelBuilder.Entity<ChatMessage>()
            .Property(m => m.Role)
            .HasMaxLength(50);

        modelBuilder.Entity<ExperimentConfig>()
            .Property(e => e.ApproachType)
            .HasMaxLength(50);
    }
}
