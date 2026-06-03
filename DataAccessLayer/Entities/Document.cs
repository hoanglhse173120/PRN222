using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class Document
{
    public int DocumentId { get; set; }

    public int SubjectId { get; set; }

    public string FileName { get; set; } = null!;

    public string? FileType { get; set; }

    public string? FilePath { get; set; }

    public long? FileSizeKb { get; set; }

    public bool? IsIndexed { get; set; }

    public DateTime? UploadedAt { get; set; }

    public virtual ICollection<DocumentChunk> DocumentChunks { get; set; } = new List<DocumentChunk>();

    public virtual Subject Subject { get; set; } = null!;
}
