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

    public string? ImageUrl { get; set; }

    // New properties for email and phone number
    public string? Email { get; set; } // Email field
    public string? PhoneNumber { get; set; } // Phone number field

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
