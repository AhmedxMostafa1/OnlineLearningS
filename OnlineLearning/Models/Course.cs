using System;
using System.Collections.Generic;

namespace OnlineLearning.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public string? CourseTitle { get; set; }

    public string? CourseDescription { get; set; }

    public int? InstructorId { get; set; }

    public int? CategoryId { get; set; }

    public bool? IsPremium { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual Instructor? Instructor { get; set; }

    public virtual ICollection<Module> Modules { get; set; } = new List<Module>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
