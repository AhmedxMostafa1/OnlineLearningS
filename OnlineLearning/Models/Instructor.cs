using System;
using System.Collections.Generic;

namespace OnlineLearning.Models;

public partial class Instructor
{
    public int InstId { get; set; }

    public string? InstFullName { get; set; }

    public string? InstEmail { get; set; }

    public string? InstPassword { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
