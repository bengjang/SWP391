using lamlai.Models;
using System;
using System.Collections.Generic;

namespace lamlai.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public decimal? DiscountAmount { get; set; }

    public decimal OriginalAmount { get; set; }

    public virtual Order Order { get; set; } = null!;
}
