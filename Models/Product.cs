﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace lamlai.Models;

public partial class Product
{
    public int ProductId { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string? ProductCode { get; set; }

    public int CategoryId { get; set; }

    public string ProductName { get; set; } = null!;    

    public int Quantity { get; set; }

    public string Capacity { get; set; } = null!;

    public decimal Price { get; set; }

    public string Brand { get; set; } = null!;

    public string Origin { get; set; } = null!;

    public string Status { get; set; } = null!;

    

    public string? SkinType { get; set; }

    public string? Description { get; set; }

    public string? Ingredients { get; set; }

    public string? UsageInstructions { get; set; }

    public DateTime? ManufactureDate { get; set; }
    public DateTime? ImportDate { get; set; } // Thêm ngày nhập hàng vào kho

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    
    // Thêm quan hệ với SkincareRoutineCategory
}
