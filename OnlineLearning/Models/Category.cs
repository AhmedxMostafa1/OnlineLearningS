using System;
using System.Collections.Generic;

namespace OnlineLearning.Models;

public partial class Category
{
    public int CategId { get; set; }

    public string CategName { get; set; } = null!;

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
