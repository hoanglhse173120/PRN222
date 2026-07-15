using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Context;

public class ChatbotDbContext : IdentityDbContext<IdentityUser>
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
    public virtual DbSet<TeacherSubject> TeacherSubjects { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Define Primary Keys for entities that do not follow the default convention
        modelBuilder.Entity<DocumentChunk>().HasKey(c => c.ChunkId);
        modelBuilder.Entity<DocumentChunk>().HasIndex(c => c.DocumentId); // Index for performance

        modelBuilder.Entity<ChatSession>().HasKey(s => s.SessionId);
        modelBuilder.Entity<ChatSession>().HasIndex(s => s.UserId); // Index for performance
        modelBuilder.Entity<ChatSession>().HasIndex(s => s.SubjectId); // Index for performance

        modelBuilder.Entity<ChatMessage>().HasKey(m => m.MessageId);
        modelBuilder.Entity<MessageSource>().HasKey(ms => ms.SourceId);
        modelBuilder.Entity<ExperimentConfig>().HasKey(e => e.ConfigId);
        modelBuilder.Entity<TestQuestion>().HasKey(q => q.QuestionId);
        modelBuilder.Entity<BenchmarkResult>().HasKey(b => b.ResultId);

        // TeacherSubject — liên kết giảng viên với môn học (tối đa 2 môn/giảng viên)
        modelBuilder.Entity<TeacherSubject>().HasKey(ts => ts.Id);
        modelBuilder.Entity<TeacherSubject>()
            .HasIndex(ts => new { ts.TeacherId, ts.SubjectId })
            .IsUnique(); // Tránh phân công trùng
        modelBuilder.Entity<TeacherSubject>()
            .HasOne(ts => ts.Subject)
            .WithMany()
            .HasForeignKey(ts => ts.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Subject → Documents (1:N, cascade delete)
        modelBuilder.Entity<Document>()
            .HasOne(d => d.Subject)
            .WithMany(s => s.Documents)
            .HasForeignKey(d => d.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Document → IdentityUser (N:1, set null)
        modelBuilder.Entity<Document>()
            .HasOne(d => d.UploadedByUser)
            .WithMany()
            .HasForeignKey(d => d.UploadedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Document → DocumentChunks (1:N, cascade delete)
        modelBuilder.Entity<DocumentChunk>()
            .HasOne(c => c.Document)
            .WithMany(d => d.DocumentChunks)
            .HasForeignKey(c => c.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        // ChatSession → ChatMessages (1:N, cascade delete)
        modelBuilder.Entity<ChatMessage>()
            .HasOne(m => m.Session)
            .WithMany(s => s.ChatMessages)
            .HasForeignKey(m => m.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // ChatSession → User (N:1, cascade delete or set null)
        modelBuilder.Entity<ChatSession>()
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ChatSession → Subject (N:1, set null)
        modelBuilder.Entity<ChatSession>()
            .HasOne(s => s.Subject)
            .WithMany()
            .HasForeignKey(s => s.SubjectId)
            .OnDelete(DeleteBehavior.SetNull);

        // ChatMessage → MessageSources (1:N, cascade delete)
        modelBuilder.Entity<MessageSource>()
            .HasOne(ms => ms.Message)
            .WithMany(m => m.MessageSources)
            .HasForeignKey(ms => ms.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        // DocumentChunk → MessageSources (1:N, no cascade to avoid multiple paths)
        modelBuilder.Entity<MessageSource>()
            .HasOne(ms => ms.Chunk)
            .WithMany(c => c.MessageSources)
            .HasForeignKey(ms => ms.ChunkId)
            .OnDelete(DeleteBehavior.NoAction);

        // ExperimentConfig → BenchmarkResults (1:N, cascade delete)
        modelBuilder.Entity<BenchmarkResult>()
            .HasOne(b => b.Config)
            .WithMany(e => e.BenchmarkResults)
            .HasForeignKey(b => b.ConfigId)
            .OnDelete(DeleteBehavior.Cascade);

        // TestQuestion → BenchmarkResults (1:N, no cascade)
        modelBuilder.Entity<BenchmarkResult>()
            .HasOne(b => b.Question)
            .WithMany(q => q.BenchmarkResults)
            .HasForeignKey(b => b.QuestionId)
            .OnDelete(DeleteBehavior.NoAction);

        // Subject → TestQuestions (1:N, nullable FK)
        modelBuilder.Entity<TestQuestion>()
            .HasOne(q => q.Subject)
            .WithMany(s => s.TestQuestions)
            .HasForeignKey(q => q.SubjectId)
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
