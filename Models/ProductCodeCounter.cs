using System;
using System.Collections.Generic;

namespace lamlai.Models;

public partial class ProductCodeCounter
{
    public string CategoryType { get; set; } = null!;

    public int? Counter { get; set; }
}
