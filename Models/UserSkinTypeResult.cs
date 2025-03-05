using System;
using System.Collections.Generic;

namespace lamlai.Models;

public partial class UserSkinTypeResult
{
    public int ResultId { get; set; }

    public int UserId { get; set; }

    public int AttemptNumber { get; set; }

    public string SkinType { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
