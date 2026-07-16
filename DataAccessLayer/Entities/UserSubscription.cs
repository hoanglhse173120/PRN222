using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace DataAccessLayer.Entities;

public class UserSubscription
{
    [Key]
    public int SubscriptionId { get; set; }

    [Required]
    public string UserId { get; set; } = null!;

    [Required]
    public int PackageId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [ForeignKey("UserId")]
    public virtual IdentityUser User { get; set; } = null!;

    [ForeignKey("PackageId")]
    public virtual Package Package { get; set; } = null!;
}
