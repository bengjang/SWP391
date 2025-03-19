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
            ImageUrl = request.ImageUrl ?? "", // Nếu không có ảnh thì để trống
            Email = request.Email, // Store email if needed
            PhoneNumber = request.PhoneNumber // Store phone number if needed
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Phản hồi đã được gửi!", conversationId = conversation.ConversationId });
    }

    // 📌 2️⃣ API: Admin trả lời phản hồi của User
    [HttpPost("reply")]
    public async Task<IActionResult> ReplyFeedback([FromBody] AdminReplyDto request)
    {
        // Find the existing conversation by ID
        var conversation = await _context.Conversations.FindAsync(request.ConversationId);
        if (conversation == null)
        {
            return NotFound(new { error = "Cuộc trò chuyện không tồn tại." });
        }

        // Create a new message for the admin's reply
        var replyMessage = new Message
        {
            ConversationId = request.ConversationId, // Use the existing conversation ID
            UserId = request.UserId, // This should be the admin's ID
            MessageContent = request.MessageContent,
            SendTime = DateTime.UtcNow,
            ImageUrl = request.ImageUrl ?? "",

            // Set default email and phone number
            Email = "beautycomsmetics@gmail.vn", // Default email
            PhoneNumber = "0956497123" // Default phone number
        };

        // Add the new message to the Messages table
        _context.Messages.Add(replyMessage);

        // Update the conversation's last updated time
        conversation.UpdateAt = DateTime.UtcNow;

        // Save changes to the database
        await _context.SaveChangesAsync();

        return Ok(new { message = "Admin đã trả lời phản hồi!" });
    }

    // 📌 3️⃣ API: Lấy danh sách phản hồi của một User
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserFeedbacks(int userId)
    {
        var conversations = await _context.Conversations
            .Include(c => c.User) // Include the User entity
            .Where(c => c.UserId == userId)
            .Select(c => new
            {
                c.ConversationId,
                c.UserId,
                UserName = c.User.Name, // Get the user's name
                Email = c.Messages.FirstOrDefault().Email, // Get the email from the first message
                PhoneNumber = c.Messages.FirstOrDefault().PhoneNumber, // Get the phone number from the first message
                MessageContent = c.Messages.OrderByDescending(m => m.SendTime).FirstOrDefault().MessageContent, // Get the content of the latest message
                SendTime = c.Messages.OrderByDescending(m => m.SendTime).FirstOrDefault().SendTime, // Get the send time of the latest message
                Status = c.Messages.Count > 1 ? "Replied" : "Pending" // Set the status based on the number of messages
            })
            .ToListAsync();

        return Ok(conversations);
    }

    // 📌 4️⃣ API: Admin lấy tất cả phản hồi chưa trả lời
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingFeedbacks()
    {
        var pendingConversations = await _context.Conversations
            .Include(c => c.User) // Include the User entity
            .Where(c => c.Messages.Count == 1) // Only include conversations with a single message (pending)
            .Select(c => new
            {
                c.ConversationId,
                c.UserId,
                UserName = c.User.Name, // Get the user's name
                Email = c.Messages.FirstOrDefault().Email, // Get the email from the first message
                PhoneNumber = c.Messages.FirstOrDefault().PhoneNumber, // Get the phone number from the first message
                MessageContent = c.Messages.FirstOrDefault().MessageContent, // Get the content of the first message
                SendTime = c.Messages.FirstOrDefault().SendTime, // Get the send time of the first message
                Status = "Pending" // Set the status to "Pending"
            })
            .ToListAsync();

        return Ok(pendingConversations);
    }

    // 📌 5️⃣ API: Lấy tất cả phản hồi
    [HttpGet("all")]
    public async Task<IActionResult> GetAllFeedbacks()
    {
        var feedbacks = await _context.Conversations
            .Include(c => c.User) // Include the User entity
            .Select(c => new
            {
                c.ConversationId,
                c.UserId,
                UserName = c.User.Name, // Get the user's name
                Role = c.User.Role, // Get the user's role
                Messages = c.Messages.Select(m => new
                {
                    m.MessageId,
                    m.MessageContent,
                    m.SendTime,
                    m.UserId,
                    m.ImageUrl,
                    Email = m.Email,
                    PhoneNumber = m.PhoneNumber,
                    IsAdmin = m.UserId != c.UserId // Determine if the message is from an admin
                }).ToList(),
                Status = c.Messages.Count > 1 ? "Replied" : "Pending" // Set the status based on the number of messages
            })
            .ToListAsync();

        return Ok(feedbacks);
    }

    // 📌 6️⃣ API: Lấy danh sách đơn hỗ trợ đã được staff trả lời
    [HttpGet("replied")]
    public async Task<IActionResult> GetRepliedSupportRequests()
    {
        var repliedRequests = await _context.Conversations
            .Include(c => c.User) // Include the User entity
            .Where(c => c.Messages.Count > 1) // Only include conversations with replies
            .Select(c => new
            {
                c.ConversationId,
                c.UserId,
                UserName = c.User.Name, // Get the user's name
                Role = c.User.Role, // Get the user's role
                Messages = c.Messages.Select(m => new
                {
                    m.MessageId,
                    m.MessageContent,
                    m.SendTime,
                    m.UserId,
                    m.ImageUrl,
                    Email = m.Email,
                    PhoneNumber = m.PhoneNumber,
                    IsAdmin = m.UserId != c.UserId // Determine if the message is from an admin
                }).ToList(),
                Status = "Replied" // Set the status to "Replied"
            })
            .ToListAsync();

        return Ok(repliedRequests);
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
        public string? Email { get; set; } // Email field
        public string? PhoneNumber { get; set; } // Phone number field
    }
}
