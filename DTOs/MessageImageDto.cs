using System;

namespace SWP391.DTOs
{
    // DTO cho thông tin ảnh trả về
    public class MessageImageDto
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public int UserId { get; set; }
        public string ImageUrl { get; set; }
        public DateTime? SendTime { get; set; }
    }
} 