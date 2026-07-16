using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace DataAccessLayer.Entities;

public class PaymentTransaction
{
    [Key]
    public int TransactionId { get; set; }

    [Required]
    public string UserId { get; set; } = null!;

    [Required]
    public int PackageId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = null!;

    [Required]
    public DateTime TransactionDate { get; set; }

    [Required]
    [MaxLength(100)]
    public string TransactionReference { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = null!; // Pending, Success, Failed

    [ForeignKey("UserId")]
    public virtual IdentityUser User { get; set; } = null!;

    [ForeignKey("PackageId")]
    public virtual Package Package { get; set; } = null!;
}
