using System;
using System.Collections.Generic;

namespace lamlai.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryType { get; set; } = null!;

    public string? CategoryName { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
