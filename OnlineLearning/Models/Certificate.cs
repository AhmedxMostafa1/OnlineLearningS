using System;
using System.Collections.Generic;

namespace OnlineLearning.Models;

public partial class Certificate
{
    public int CertiId { get; set; }

    public int? StudentId { get; set; }

    public int? CourseId { get; set; }

    public string? CertificatePath { get; set; }

    public DateTime? CertiIssuedDate { get; set; }

    public virtual Course? Course { get; set; }

    public virtual Student? Student { get; set; }
}
