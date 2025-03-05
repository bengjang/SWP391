using System;
using System.Collections.Generic;

namespace lamlai.Models;

public partial class Message
{
    public int MessageId { get; set; }

    public int ConversationId { get; set; }

    public int UserId { get; set; }

    public string MessageContent { get; set; } = null!;

    public DateTime SendTime { get; set; }

    public bool IsRead { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime DeletedAt { get; set; }

    public string ImageUrl { get; set; } = null!;

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
