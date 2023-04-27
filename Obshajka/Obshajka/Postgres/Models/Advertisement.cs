using System;
using System.Collections.Generic;

namespace Obshajka.Postgres.Models;

public partial class Advertisement
{
    public long Id { get; set; }

    public long CreatorId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int DormitoryId { get; set; }

    public int? Price { get; set; }

    public string? Image { get; set; }

    public DateOnly DateOfAddition { get; set; }

    public virtual User Creator { get; set; } = null!;
}
