using System;
using System.Collections.Generic;

namespace OnlineLearning.Models;

public partial class Quiz
{
    public int QuizId { get; set; }

    public int? ModuleId { get; set; }

    public string? Question { get; set; }

    public string? OptionA { get; set; }

    public string? OptionB { get; set; }

    public string? OptionC { get; set; }

    public string? OptionD { get; set; }

    public string? CorrectOption { get; set; }

    public virtual Module? Module { get; set; }
}
