using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Entities;

public partial class ChatbotRbldbContext : DbContext
{
    public ChatbotRbldbContext()
    {
    }

    public ChatbotRbldbContext(DbContextOptions<ChatbotRbldbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<BenchmarkResult> BenchmarkResults { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<ChatSession> ChatSessions { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<DocumentChunk> DocumentChunks { get; set; }

    public virtual DbSet<ExperimentConfig> ExperimentConfigs { get; set; }

    public virtual DbSet<MessageSource> MessageSources { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<TestQuestion> TestQuestions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=HOANG\\SQLEXPRESS;uid=sa;pwd=12345;database=ChatbotRBLDb;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.ProviderKey).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<BenchmarkResult>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PK__Benchmar__976902284704427A");

            entity.Property(e => e.ResultId).HasColumnName("ResultID");
            entity.Property(e => e.ConfigId).HasColumnName("ConfigID");
            entity.Property(e => e.EvaluatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");

            entity.HasOne(d => d.Config).WithMany(p => p.BenchmarkResults)
                .HasForeignKey(d => d.ConfigId)
                .HasConstraintName("FK__Benchmark__Confi__778AC167");

            entity.HasOne(d => d.Question).WithMany(p => p.BenchmarkResults)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Benchmark__Quest__787EE5A0");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__ChatMess__C87C037CD788D581");

            entity.Property(e => e.MessageId).HasColumnName("MessageID");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SessionId).HasColumnName("SessionID");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Session).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.SessionId)
                .HasConstraintName("FK__ChatMessa__Sessi__797309D9");
        });

        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PK__ChatSess__C9F49270868AE3B9");

            entity.HasIndex(e => e.UserId, "IX_ChatSessions_UserId");
            entity.HasIndex(e => e.SubjectId, "IX_ChatSessions_SubjectId");

            entity.Property(e => e.SessionId).HasColumnName("SessionID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SessionName)
                .HasMaxLength(255)
                .HasDefaultValue("Phiên trò chuyện mới");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__Document__1ABEEF6F2A17520E");

            entity.Property(e => e.DocumentId).HasColumnName("DocumentID");
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FileSizeKb).HasColumnName("FileSizeKB");
            entity.Property(e => e.FileType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IsIndexed).HasDefaultValue(false);
            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Subject).WithMany(p => p.Documents)
                .HasForeignKey(d => d.SubjectId)
                .HasConstraintName("FK__Documents__Subje__7B5B524B");
        });

        modelBuilder.Entity<DocumentChunk>(entity =>
        {
            entity.HasKey(e => e.ChunkId).HasName("PK__Document__FBFF9D208483EF32");

            entity.HasIndex(e => e.DocumentId, "IX_DocumentChunks_DocumentId");

            entity.Property(e => e.ChunkId).HasColumnName("ChunkID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DocumentId).HasColumnName("DocumentID");

            entity.HasOne(d => d.Document).WithMany(p => p.DocumentChunks)
                .HasForeignKey(d => d.DocumentId)
                .HasConstraintName("FK__DocumentC__Docum__7A672E12");
        });

        modelBuilder.Entity<ExperimentConfig>(entity =>
        {
            entity.HasKey(e => e.ConfigId).HasName("PK__Experime__C3BC333C11498611");

            entity.Property(e => e.ConfigId).HasColumnName("ConfigID");
            entity.Property(e => e.ApproachType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ChunkingStrategy).HasMaxLength(100);
            entity.Property(e => e.ConfigName).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EmbeddingModel).HasMaxLength(100);
        });

        modelBuilder.Entity<MessageSource>(entity =>
        {
            entity.HasKey(e => e.SourceId).HasName("PK__MessageS__16E019F9CCFF5107");

            entity.Property(e => e.SourceId).HasColumnName("SourceID");
            entity.Property(e => e.ChunkId).HasColumnName("ChunkID");
            entity.Property(e => e.MessageId).HasColumnName("MessageID");

            entity.HasOne(d => d.Chunk).WithMany(p => p.MessageSources)
                .HasForeignKey(d => d.ChunkId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MessageSo__Chunk__7C4F7684");

            entity.HasOne(d => d.Message).WithMany(p => p.MessageSources)
                .HasForeignKey(d => d.MessageId)
                .HasConstraintName("FK__MessageSo__Messa__7D439ABD");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.SubjectId).HasName("PK__Subjects__AC1BA3880B4BDFCA");

            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SubjectName).HasMaxLength(255);
        });

        modelBuilder.Entity<TestQuestion>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__TestQues__0DC06F8CA08F0175");

            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");

            entity.HasOne(d => d.Subject).WithMany(p => p.TestQuestions)
                .HasForeignKey(d => d.SubjectId)
                .HasConstraintName("FK__TestQuest__Subje__7E37BEF6");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
