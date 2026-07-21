using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Context;

public class ChatbotDbContext : IdentityDbContext<IdentityUser>
{
    public ChatbotDbContext(DbContextOptions<ChatbotDbContext> options) : base(options) { }

    /// <summary> Bảng lưu trữ danh sách các môn học </summary>
    public DbSet<Subject> Subjects { get; set; }
    /// <summary> Bảng lưu trữ các tài liệu được upload lên hệ thống </summary>
    public DbSet<Document> Documents { get; set; }
    /// <summary> Bảng chứa các đoạn văn bản (chunks) được cắt từ tài liệu để phục vụ Vector Search (RAG) </summary>
    public DbSet<DocumentChunk> DocumentChunks { get; set; }
    /// <summary> Bảng quản lý các phiên chat (session) của người dùng </summary>
    public DbSet<ChatSession> ChatSessions { get; set; }
    /// <summary> Bảng lưu chi tiết từng tin nhắn nhận/gửi trong một phiên chat </summary>
    public DbSet<ChatMessage> ChatMessages { get; set; }
    /// <summary> Bảng theo dõi các đoạn văn bản/nguồn trích dẫn được AI sử dụng để sinh câu trả lời </summary>
    public DbSet<MessageSource> MessageSources { get; set; }
    /// <summary> Bảng lưu cấu hình thí nghiệm (Benchmark/Experiment Configs) </summary>
    public DbSet<ExperimentConfig> ExperimentConfigs { get; set; }
    /// <summary> Bảng lưu danh sách các câu hỏi test dùng cho Benchmark </summary>
    public DbSet<TestQuestion> TestQuestions { get; set; }
    /// <summary> Bảng lưu kết quả đánh giá (chấm điểm) của AI qua các đợt Benchmark </summary>
    public DbSet<BenchmarkResult> BenchmarkResults { get; set; }
    /// <summary> Bảng lưu thông tin phân công môn học cho giảng viên (N-N) </summary>
    public virtual DbSet<TeacherSubject> TeacherSubjects { get; set; }
    /// <summary> Bảng các gói dịch vụ/đăng ký trả phí (Packages) </summary>
    public DbSet<Package> Packages { get; set; }
    /// <summary> Bảng lưu trạng thái Subscription (kích hoạt gói) của từng người dùng </summary>
    public DbSet<UserSubscription> UserSubscriptions { get; set; }
    /// <summary> Bảng lịch sử các biên lai giao dịch thanh toán </summary>
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
    /// <summary> Bảng cấu hình tùy biến cơ chế chia tách văn bản (Chunking) </summary>
    public DbSet<ChunkingConfig> ChunkingConfigs { get; set; }

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
        modelBuilder.Entity<ChunkingConfig>().HasKey(c => c.Id);

        modelBuilder.Entity<Package>().Property(p => p.Price).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<PaymentTransaction>().Property(p => p.Amount).HasColumnType("decimal(18,2)");

        // Seed default config
        modelBuilder.Entity<ChunkingConfig>().HasData(new ChunkingConfig
        {
            Id = 1,
            Strategy = "Words",
            MaxSize = 500,
            Overlap = 50,
            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Local) // Seeded base time
        });

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

        // UserSubscription Mappings
        modelBuilder.Entity<UserSubscription>()
            .HasOne(us => us.User)
            .WithMany()
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserSubscription>()
            .HasOne(us => us.Package)
            .WithMany()
            .HasForeignKey(us => us.PackageId)
            .OnDelete(DeleteBehavior.Cascade);

        // PaymentTransaction Mappings
        modelBuilder.Entity<PaymentTransaction>()
            .HasOne(pt => pt.User)
            .WithMany()
            .HasForeignKey(pt => pt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PaymentTransaction>()
            .HasOne(pt => pt.Package)
            .WithMany()
            .HasForeignKey(pt => pt.PackageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
