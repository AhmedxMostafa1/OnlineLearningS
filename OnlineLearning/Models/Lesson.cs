using System;
using System.Collections.Generic;

namespace OnlineLearning.Models;

public partial class Lesson
{
    public int LessonId { get; set; }

    public string? LessonTitle { get; set; }

    public string? LessonContentUrl { get; set; }

    public string? Type { get; set; }

    public int? ModuleId { get; set; }

    public virtual Module? Module { get; set; }
}
