using System;
using System.Collections.Generic;

namespace OnlineLearning.Models;

public partial class Payment
{
    public int PayId { get; set; }

    public int? StudentId { get; set; }

    public int? CourseId { get; set; }

    public decimal? PayAmount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public virtual Course? Course { get; set; }

    public virtual Student? Student { get; set; }
}
