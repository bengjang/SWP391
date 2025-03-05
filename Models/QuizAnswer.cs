using System;
using System.Collections.Generic;

namespace lamlai2.Models;

public partial class QuizAnswer
{
    public int AnswerId { get; set; }

    public int QuestionId { get; set; }

    public string AnswerText { get; set; } = null!;

    public string SkinType { get; set; } = null!;

    public virtual QuizQuestion Question { get; set; } = null!;

    public virtual ICollection<UserQuizResponse> UserQuizResponses { get; set; } = new List<UserQuizResponse>();
}
