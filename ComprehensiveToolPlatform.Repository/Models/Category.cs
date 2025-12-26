using System;
using System.Collections.Generic;

namespace ComprehensiveToolPlatform.Repository.Models;

public partial class Category
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int Sort { get; set; }

    public bool IsActive { get; set; }
}
