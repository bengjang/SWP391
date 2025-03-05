using System;
using System.Collections.Generic;

namespace lamlai.Models;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public int ProductId { get; set; }

    public string EventName { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Description { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string Condition { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
