using System;
using System.ComponentModel.DataAnnotations;

namespace lamlai.Models;

public partial class SkincareRoutine
{
    public int RoutineId { get; set; }

    [Required]
    [MaxLength(50)]
    public string SkinType { get; set; } = null!;

    public int UserId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    [Required]
    public string Content { get; set; } = null!;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual User User { get; set; } = null!;
}
