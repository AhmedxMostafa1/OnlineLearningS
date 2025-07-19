using System;
using System.Collections.Generic;

namespace OnlineLearning.Models;

public partial class Module
{
    public int ModuleId { get; set; }

    public string? ModuleTitle { get; set; }

    public int? CourseId { get; set; }

    public virtual Course? Course { get; set; }

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}
