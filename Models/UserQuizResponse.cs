using System;
using System.Collections.Generic;

namespace lamlai.Models;

public partial class UserQuizResponse
{
    public int ResponseId { get; set; }

    public int UserId { get; set; }

    public int QuestionId { get; set; }

    public int SelectedAnswerId { get; set; }

    public DateTime? AnsweredAt { get; set; }

    public virtual QuizQuestion Question { get; set; } = null!;

    public virtual QuizAnswer SelectedAnswer { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
