using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace lamlai.Models;

public partial class Post
{
    public int PostId { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
} 