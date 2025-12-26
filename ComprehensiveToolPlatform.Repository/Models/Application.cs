using System;
using System.Collections.Generic;

namespace ComprehensiveToolPlatform.Repository.Models;

public partial class Application
{
    public string Id { get; set; } = null!;

    public string CategoryId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int Sort { get; set; }

    public bool IsActive { get; set; }

    public string? FileType { get; set; }

    public int? FileSize { get; set; }
}
