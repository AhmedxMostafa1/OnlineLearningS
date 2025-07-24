using System;
using System.Collections.Generic;

namespace OnlineLearning.Models;

public partial class PendingInstructor
{
    public int PendingInstId { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public DateTime? AppliedAt { get; set; }
}
