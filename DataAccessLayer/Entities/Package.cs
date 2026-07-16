using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public class Package
{
    [Key]
    public int PackageId { get; set; }

    [Required]
    [MaxLength(100)]
    public string PackageName { get; set; } = null!;

    [Required]
    public decimal Price { get; set; }

    [Required]
    public int DurationInDays { get; set; }

    [Required]
    public int MaxQuestionsPerDay { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;
}
