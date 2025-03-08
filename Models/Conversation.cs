using System;
using System.Collections.Generic;

namespace lamlai.Models;

public partial class Conversation
{
    public int ConversationId { get; set; }

    public int UserId { get; set; }

    public DateTime UpdateAt { get; set; }

    public int LastMessageId { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual User User { get; set; } = null!;
}
