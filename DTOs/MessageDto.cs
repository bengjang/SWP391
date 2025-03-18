using System;

namespace SWP391.DTOs
{
    // DTO cho tin nhắn có ảnh base64
    public class MessageDto
    {
        public int? UserId { get; set; }
        public int? ConversationId { get; set; }
        public string MessageContent { get; set; }
        public string ImageData { get; set; } // Base64 image data
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
} 