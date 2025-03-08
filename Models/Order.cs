using lamlai.Models;
using System;
using System.Collections.Generic;

namespace lamlai.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public DateTime OrderDate { get; set; }

    public string OrderStatus { get; set; } = null!;

    public string? DeliveryStatus { get; set; }

    public string? DeliveryAddress { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Note { get; set; }

    public int? VoucherId { get; set; }

    public virtual ICollection<CancelRequest> CancelRequests { get; set; } = new List<CancelRequest>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual User User { get; set; } = null!;

    public virtual Voucher? Voucher { get; set; }
}
