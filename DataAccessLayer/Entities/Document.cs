using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public partial class Document
{
    public int DocumentId { get; set; }

    [Required]
    public int SubjectId { get; set; }

    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = null!;

    [StringLength(50)]
    public string? FileType { get; set; }

    [StringLength(1000)]
    public string? FilePath { get; set; }

    public long? FileSizeKb { get; set; }

    public bool? IsIndexed { get; set; }

    public DateTime? UploadedAt { get; set; }

    [StringLength(450)]
    public string? UploadedByUserId { get; set; }

    public virtual Microsoft.AspNetCore.Identity.IdentityUser? UploadedByUser { get; set; }

    public virtual ICollection<DocumentChunk> DocumentChunks { get; set; } = new List<DocumentChunk>();

    public virtual Subject Subject { get; set; } = null!;
}
