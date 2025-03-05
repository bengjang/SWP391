using System;
using System.Collections.Generic;

namespace lamlai.Models;

public partial class Voucher
{
    public int VoucherId { get; set; }

    public int UserId { get; set; }

    public string VoucherName { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Description { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string Condition { get; set; } = null!;

    public int Quantity { get; set; }

    public virtual User User { get; set; } = null!;
}
