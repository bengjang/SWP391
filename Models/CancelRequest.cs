using System;
using System.Collections.Generic;

namespace lamlai.Models;

public partial class CancelRequest
{
    public int CancelRequestId { get; set; }

    public int OrderId { get; set; }

    public string FullName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Reason { get; set; } = null!;

    public DateTime RequestDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
