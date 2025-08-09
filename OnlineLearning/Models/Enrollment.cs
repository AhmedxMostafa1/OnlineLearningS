using System;
using System.Collections.Generic;

namespace OnlineLearning.Models;

public partial class Enrollment
{
    public int EnrId { get; set; }

    public int? StudentId { get; set; }

    public int? CourseId { get; set; }

    public DateTime? EnrollDate { get; set; }

    public bool? CompletionStatus { get; set; }

    public string? PaymentStatus { get; set; }

    public virtual Course? Course { get; set; }

    public virtual Student? Student { get; set; }
}
