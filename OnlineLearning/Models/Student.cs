using System;
using System.Collections.Generic;

namespace OnlineLearning.Models;

public partial class Student
{
    public int StuId { get; set; }

    public string? StuFullName { get; set; }

    public string? StuEmail { get; set; }

    public string? StuPassword { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
