using System;
using Microsoft.AspNetCore.Http;

namespace SWP391.DTOs
{
    // DTO cho tin nhắn với form upload ảnh
    public class MessageFormDto
    {
        public int? UserId { get; set; }
        public int? ConversationId { get; set; }
        public string MessageContent { get; set; }
        public IFormFile Image { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
} 