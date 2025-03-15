using lamlai.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

[Route("api/feedbacks")]
[ApiController]
public class FeedbackController : ControllerBase
{
    private readonly TestContext _context;

    public FeedbackController(TestContext context)
    {
        _context = context;
    }

    // 📌 1️⃣ API: Người dùng gửi phản hồi cho Admin
    [HttpPost("send")]
    public async Task<IActionResult> SendFeedback([FromBody] FeedbackRequestDto request)
    {
        var conversation = new Conversation
        {
            UserId = request.UserId,
            UpdateAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var message = new Message
        {
            ConversationId = conversation.ConversationId,
            UserId = request.UserId,
            MessageContent = request.MessageContent,
            SendTime = DateTime.UtcNow,
            ImageUrl = request.ImageUrl ?? "" // Nếu không có ảnh thì để trống
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Phản hồi đã được gửi!", conversationId = conversation.ConversationId });
    }

    // 📌 2️⃣ API: Admin trả lời phản hồi của User
    [HttpPost("reply")]
    public async Task<IActionResult> ReplyFeedback([FromBody] AdminReplyDto request)
    {
        var conversation = await _context.Conversations.FindAsync(request.ConversationId);
        if (conversation == null)
        {
            return NotFound(new { error = "Cuộc trò chuyện không tồn tại." });
        }

        var replyMessage = new Message
        {
            ConversationId = request.ConversationId,
            UserId = request.UserId,
            MessageContent = request.MessageContent,
            SendTime = DateTime.UtcNow,
            ImageUrl = request.ImageUrl ?? ""
        };

        _context.Messages.Add(replyMessage);
        conversation.UpdateAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Admin đã trả lời phản hồi!" });
    }

    // 📌 3️⃣ API: Lấy danh sách phản hồi của một User
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserFeedbacks(int userId)
    {
        var conversations = await _context.Conversations
            .Where(c => c.UserId == userId)
            .Select(c => new
            {
                c.ConversationId,
                c.UpdateAt,
                Messages = c.Messages.OrderBy(m => m.SendTime)
                    .Select(m => new
                    {
                        m.MessageId,
                        m.MessageContent,
                        m.SendTime,
                        m.UserId,
                        m.ImageUrl
                    }).ToList()
            })
            .ToListAsync();

        return Ok(conversations);
    }

    // 📌 4️⃣ API: Admin lấy tất cả phản hồi chưa trả lời
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingFeedbacks()
    {
        var pendingConversations = await _context.Conversations
            .Where(c => c.Messages.Count == 1)
            .Select(c => new
            {
                c.ConversationId,
                c.UserId,
                c.UpdateAt,
                FirstMessage = c.Messages.Any() ? c.Messages.OrderBy(m => m.SendTime).First().MessageContent : "No messages yet"
            })
            .ToListAsync();

        return Ok(pendingConversations);
    }

    // 📌 DTOs
    public class AdminReplyDto
    {
        public int ConversationId { get; set; }
        public int UserId { get; set; }
        public string MessageContent { get; set; } = null!;
        public string? ImageUrl { get; set; } // Ảnh kèm theo tin nhắn
    }

    public class FeedbackRequestDto
    {
        public int UserId { get; set; }
        public string MessageContent { get; set; } = null!;
        public string? ImageUrl { get; set; } // Ảnh kèm theo tin nhắn
    }
}
