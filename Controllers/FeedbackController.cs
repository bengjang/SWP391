using lamlai.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using SWP391.Services;
using CloudinaryDotNet.Actions;
using System.Net.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;

[Route("api/feedbacks")]
[ApiController]
public class FeedbackController : ControllerBase
{
    private readonly TestContext _context;
    private readonly IPhotoService _photoService;

    public FeedbackController(TestContext context, IPhotoService photoService)
    {
        _context = context;
        _photoService = photoService;
    }

    // 📌 1️⃣ API: Người dùng gửi phản hồi cho Admin
    [HttpPost("send")]
    public async Task<IActionResult> SendFeedback([FromBody] FeedbackRequestDto request)
    {
        try 
        {
            // Ghi log thông tin request để debug
            Console.WriteLine($"Received feedback: UserId={request.UserId}, ImageUrl={request.ImageUrl ?? "null"}");
            
            var conversation = new Conversation
            {
                UserId = request.UserId,
                UpdateAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"Created conversation with ID: {conversation.ConversationId}");

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
            
            Console.WriteLine($"Created message with ID: {message.MessageId}, ImageUrl: {message.ImageUrl}");

            return Ok(new { message = "Phản hồi đã được gửi!", conversationId = conversation.ConversationId });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending feedback: {ex.Message}");
            return StatusCode(500, new { error = $"Lỗi khi gửi phản hồi: {ex.Message}" });
        }
    }

    // 📌 2️⃣ API: Admin trả lời phản hồi của User
    [HttpPost("reply")]
    public async Task<IActionResult> ReplyFeedback([FromBody] AdminReplyDto request)
    {
        try
        {
            // Ghi log thông tin request để debug
            Console.WriteLine($"Received reply: ConversationId={request.ConversationId}, UserId={request.UserId}, ImageUrl={request.ImageUrl ?? "null"}");
            
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
            
            Console.WriteLine($"Created reply message with ID: {replyMessage.MessageId}, ImageUrl: {replyMessage.ImageUrl}");

            return Ok(new { message = "Admin đã trả lời phản hồi!" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error replying to feedback: {ex.Message}");
            return StatusCode(500, new { error = $"Lỗi khi trả lời phản hồi: {ex.Message}" });
        }
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

    [HttpGet("replied/{userId}")]
    public async Task<IActionResult> GetRepliedSupportRequestsByUser(int userId)
    {
        var repliedRequests = await _context.Conversations
            .Include(c => c.User)
            .Where(c => c.UserId == userId && c.Messages.Count > 1)
            .Select(c => new
            {
                c.ConversationId,
                c.UserId,
                UserName = c.User.Name,
                Email = c.Messages.FirstOrDefault().Email,
                PhoneNumber = c.Messages.FirstOrDefault().PhoneNumber,
                MessageContent = c.Messages.OrderByDescending(m => m.SendTime).FirstOrDefault().MessageContent,
                SendTime = c.Messages.OrderByDescending(m => m.SendTime).FirstOrDefault().SendTime,
                Messages = c.Messages
                    .OrderBy(m => m.SendTime) // Thêm sắp xếp tin nhắn theo thời gian tăng dần
                    .Select(m => new
                    {
                        m.MessageId,
                        m.MessageContent,
                        m.SendTime,
                        m.UserId,
                        m.ImageUrl,
                        Email = m.Email,
                        PhoneNumber = m.PhoneNumber,
                        IsAdmin = m.UserId != c.UserId
                    })
                    .ToList(),
                Status = "Replied"
            })
            .ToListAsync();

        return Ok(repliedRequests);
    }

    // 📌 7️⃣ API: Upload hình ảnh cho phản hồi (sử dụng Cloudinary)
    [HttpPost("upload/image")]
    public async Task<IActionResult> UploadImage(IFormFile image)
    {
        try
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest(new { error = "Không có file được gửi lên." });
            }

            // Kiểm tra kích thước file (giới hạn 5MB)
            if (image.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { error = "Kích thước file không được vượt quá 5MB." });
            }

            // Kiểm tra định dạng file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { error = "Chỉ chấp nhận định dạng ảnh: jpg, jpeg, png, gif, webp." });
            }

            // Upload ảnh lên Cloudinary
            var uploadResult = await _photoService.AddPhotoAsync(image);
            if (uploadResult.Error != null)
            {
                return BadRequest(new { error = $"Lỗi khi tải lên ảnh: {uploadResult.Error.Message}" });
            }

            // Lấy URL của ảnh từ kết quả upload
            string imageUrl = uploadResult.SecureUrl.AbsoluteUri;
            
            // Ghi log URL ảnh để debug
            Console.WriteLine($"Uploaded image URL: {imageUrl}");
            
            // Trả về URL ảnh chuẩn nhất quán dưới dạng string trong trường "imageUrl"
            return Ok(imageUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading image: {ex.Message}");
            return StatusCode(500, new { error = $"Lỗi khi tải lên ảnh: {ex.Message}" });
        }
    }

    // 📌 8️⃣ API: Lấy ảnh phản hồi - không cần vì ảnh được lưu trên Cloudinary
    [HttpGet("image/{imageUrl}")]
    public async Task<IActionResult> GetFeedbackImage(string imageUrl)
    {
        try
        {
            // Ảnh được lưu trên Cloudinary, chỉ cần chuyển hướng đến URL
            if (Uri.TryCreate(imageUrl, UriKind.Absolute, out Uri uri))
            {
                return Redirect(uri.ToString());
            }
            else
            {
                return BadRequest(new { error = "URL ảnh không hợp lệ." });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Lỗi khi lấy ảnh: {ex.Message}" });
        }
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
    
    // DTO cho API multipart/form-data
    public class FeedbackWithImageDto
    {
        public IFormFile? Image { get; set; }
        public int UserId { get; set; }
        public string MessageContent { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }
    
    public class ReplyWithImageDto
    {
        public IFormFile? Image { get; set; }
        public int ConversationId { get; set; }
        public int UserId { get; set; }
        public string MessageContent { get; set; } = string.Empty;
    }
    
    // 📌 9️⃣ API: Gửi phản hồi kèm ảnh (multipart/form-data)
    [HttpPost("send-with-image")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SendFeedbackWithImage([FromForm] FeedbackWithImageDto request)
    {
        try
        {
            Console.WriteLine($"Received feedback with image: UserId={request.UserId}, HasImage={request.Image != null}");
            
            // Tạo conversation mới
            var conversation = new Conversation
            {
                UserId = request.UserId,
                UpdateAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"Created conversation with ID: {conversation.ConversationId}");
            
            string imageUrl = "";
            
            // Upload ảnh lên Cloudinary nếu có
            if (request.Image != null && request.Image.Length > 0)
            {
                // Kiểm tra kích thước file (giới hạn 5MB)
                if (request.Image.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { error = "Kích thước ảnh không được vượt quá 5MB." });
                }

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(request.Image.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { error = "Chỉ chấp nhận định dạng ảnh: jpg, jpeg, png, gif, webp." });
                }

                // Upload ảnh lên Cloudinary
                var uploadResult = await _photoService.AddPhotoAsync(request.Image);
                if (uploadResult.Error != null)
                {
                    return BadRequest(new { error = $"Lỗi khi tải lên ảnh: {uploadResult.Error.Message}" });
                }

                // Lấy URL của ảnh từ kết quả upload
                imageUrl = uploadResult.SecureUrl.AbsoluteUri;
                Console.WriteLine($"Uploaded image URL: {imageUrl}");
            }

            // Tạo message mới
            var message = new Message
            {
                ConversationId = conversation.ConversationId,
                UserId = request.UserId,
                MessageContent = request.MessageContent,
                SendTime = DateTime.UtcNow,
                ImageUrl = imageUrl, // URL ảnh Cloudinary hoặc rỗng nếu không có ảnh
                Email = request.Email ?? "",
                PhoneNumber = request.PhoneNumber ?? ""
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"Created message with ID: {message.MessageId}, ImageUrl: {message.ImageUrl}");

            return Ok(new { 
                message = "Phản hồi đã được gửi thành công!", 
                conversationId = conversation.ConversationId,
                imageUrl = imageUrl // Trả về URL ảnh cho client nếu cần
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending feedback with image: {ex.Message}");
            return StatusCode(500, new { error = $"Lỗi khi gửi phản hồi: {ex.Message}" });
        }
    }
    
    // 📌 🔟 API: Admin trả lời kèm ảnh (multipart/form-data)
    [HttpPost("reply-with-image")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReplyFeedbackWithImage([FromForm] ReplyWithImageDto request)
    {
        try
        {
            Console.WriteLine($"Received reply with image: ConversationId={request.ConversationId}, UserId={request.UserId}, HasImage={request.Image != null}");
            
            // Tìm conversation
            var conversation = await _context.Conversations.FindAsync(request.ConversationId);
            if (conversation == null)
            {
                return NotFound(new { error = "Cuộc trò chuyện không tồn tại." });
            }
            
            string imageUrl = "";
            
            // Upload ảnh lên Cloudinary nếu có
            if (request.Image != null && request.Image.Length > 0)
            {
                // Kiểm tra kích thước file (giới hạn 5MB)
                if (request.Image.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { error = "Kích thước ảnh không được vượt quá 5MB." });
                }

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(request.Image.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { error = "Chỉ chấp nhận định dạng ảnh: jpg, jpeg, png, gif, webp." });
                }

                // Upload ảnh lên Cloudinary
                var uploadResult = await _photoService.AddPhotoAsync(request.Image);
                if (uploadResult.Error != null)
                {
                    return BadRequest(new { error = $"Lỗi khi tải lên ảnh: {uploadResult.Error.Message}" });
                }

                // Lấy URL của ảnh từ kết quả upload
                imageUrl = uploadResult.SecureUrl.AbsoluteUri;
                Console.WriteLine($"Uploaded image URL: {imageUrl}");
            }

            // Tạo message phản hồi
            var replyMessage = new Message
            {
                ConversationId = request.ConversationId,
                UserId = request.UserId,
                MessageContent = request.MessageContent,
                SendTime = DateTime.UtcNow,
                ImageUrl = imageUrl, // URL ảnh Cloudinary hoặc rỗng nếu không có ảnh
                Email = "beautycomsmetics@gmail.vn", // Default email
                PhoneNumber = "0956497123" // Default phone number
            };

            _context.Messages.Add(replyMessage);
            
            // Cập nhật thời gian cập nhật cuộc trò chuyện
            conversation.UpdateAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"Created reply message with ID: {replyMessage.MessageId}, ImageUrl: {replyMessage.ImageUrl}");

            return Ok(new { 
                message = "Phản hồi đã được gửi thành công!", 
                messageId = replyMessage.MessageId,
                imageUrl = imageUrl // Trả về URL ảnh cho client nếu cần
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error replying with image: {ex.Message}");
            return StatusCode(500, new { error = $"Lỗi khi gửi phản hồi: {ex.Message}" });
        }
    }
}
