using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public partial class Subject
{
    public int SubjectId { get; set; }

    [Required]
    [StringLength(100)]
    public string SubjectName { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<TestQuestion> TestQuestions { get; set; } = new List<TestQuestion>();
}
