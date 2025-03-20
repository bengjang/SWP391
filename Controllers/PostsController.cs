using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using lamlai.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace test2.Controllers
{
    [Route("api/Post")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly TestContext _context;
        private readonly Cloudinary _cloudinary;

        public PostsController(TestContext context, IConfiguration configuration)
        {
            _context = context;
            
            // Khởi tạo Cloudinary với xử lý lỗi
            try
            {
                // Kiểm tra cả hai quy ước cấu hình
                var cloudName = configuration["Cloudinary:CloudName"];
                var apiKey = configuration["Cloudinary:ApiKey"];
                var apiSecret = configuration["Cloudinary:ApiSecret"];

                // Nếu không tìm thấy cấu hình "Cloudinary", thử "CloudinarySettings"
                if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
                {
                    cloudName = configuration["CloudinarySettings:CloudName"];
                    apiKey = configuration["CloudinarySettings:ApiKey"];
                    apiSecret = configuration["CloudinarySettings:ApiSecret"];
                    
                    if (!string.IsNullOrEmpty(cloudName) && !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(apiSecret))
                    {
                        Console.WriteLine("Sử dụng cấu hình CloudinarySettings");
                    }
                }
                else
                {
                    Console.WriteLine("Sử dụng cấu hình Cloudinary");
                }

                // Nếu vẫn không tìm thấy, sử dụng giá trị mặc định
                if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
                {
                    // Thay thế bằng thông tin Cloudinary thực tế
                    cloudName = "dlallsibu";
                    apiKey = "315293888899118";
                    apiSecret = "ucpV0lb_e0D7dmPRLFl3A94jumw";
                    
                    Console.WriteLine("Sử dụng thông tin Cloudinary mặc định vì thiếu cấu hình trong appsettings.json");
                }

                var cloudinaryAccount = new Account(cloudName, apiKey, apiSecret);
                _cloudinary = new Cloudinary(cloudinaryAccount);
            }
            catch (Exception ex)
            {
                // Log lỗi và tạo Cloudinary với thông tin cụ thể
                Console.WriteLine($"Lỗi khởi tạo Cloudinary: {ex.Message}");
                
                // Sử dụng thông tin từ appsettings.json
                var cloudinaryAccount = new Account(
                    "dlallsibu",
                    "315293888899118",
                    "ucpV0lb_e0D7dmPRLFl3A94jumw"
                );
                _cloudinary = new Cloudinary(cloudinaryAccount);
            }
        }

        // GET: api/Post
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
        {
            try
            {
                // Truy vấn lấy tất cả bài viết, giống như câu lệnh SQL:
                // SELECT TOP (1000) [PostId], [UserId], [Title], [Content], [ImageUrl], [CreatedAt] 
                // FROM [blog].[dbo].[Posts]
                // Kèm theo thông tin người dùng và sắp xếp theo thời gian giảm dần
                var posts = await _context.Posts
                    .Include(p => p.User) // Lấy thông tin người dùng liên quan
                    .OrderByDescending(p => p.CreatedAt) // Sắp xếp theo thời gian tạo giảm dần (mới nhất lên đầu)
                    .ToListAsync();

                // Log số lượng bài viết đã tìm thấy để debug
                Console.WriteLine($"Đã tìm thấy {posts.Count} bài viết");
                
                // Kiểm tra và xử lý dữ liệu null/empty
                foreach (var post in posts)
                {
                    // Đảm bảo Title không null
                    if (string.IsNullOrEmpty(post.Title))
                    {
                        post.Title = "Không có tiêu đề";
                    }
                    
                    // Đảm bảo Content không null
                    if (post.Content == null)
                    {
                        post.Content = "";
                    }
                    
                    // Log thông tin của post
                    Console.WriteLine($"Post ID: {post.PostId}, Title: {post.Title}, UserId: {post.UserId}, Has Image: {!string.IsNullOrEmpty(post.ImageUrl)}");
                }

                // Chuyển đổi thành mảng JSON thuần túy với chính xác các trường từ database
                // PostId, UserId, Title, Content, ImageUrl, CreatedAt
                var simplePostsArray = posts.Select(p => new 
                {
                    PostId = p.PostId,
                    UserId = p.UserId,
                    Title = p.Title,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    // Thêm thông tin User nếu cần thiết cho frontend
                    User = p.User != null ? new 
                    {
                        UserId = p.User.UserId,
                        FullName = p.User.FullName,
                        Email = p.User.Email
                    } : null
                }).ToList();

                return Ok(simplePostsArray);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy danh sách bài viết: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, $"Đã xảy ra lỗi khi lấy danh sách bài viết: {ex.Message}");
            }
        }

        // GET: api/Post/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(int id)
        {
            try
            {
                Console.WriteLine($"Yêu cầu lấy bài viết ID: {id}");
                
                // Truy vấn tương tự như:
                // SELECT [PostId], [UserId], [Title], [Content], [ImageUrl], [CreatedAt] 
                // FROM [blog].[dbo].[Posts]
                // WHERE [PostId] = {id}
                var post = await _context.Posts
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.PostId == id);

                if (post == null)
                {
                    Console.WriteLine($"Không tìm thấy bài viết ID: {id}");
                    return NotFound("Không tìm thấy bài viết");
                }

                // Đảm bảo Title không null
                if (string.IsNullOrEmpty(post.Title))
                {
                    post.Title = "Không có tiêu đề";
                }
                
                // Đảm bảo Content không null
                if (post.Content == null)
                {
                    post.Content = "";
                }
                
                // Log thông tin của post
                Console.WriteLine($"Đã tìm thấy bài viết - ID: {post.PostId}, Title: {post.Title}, UserId: {post.UserId}");

                // Chuyển đổi post thành JSON với chính xác các trường từ database
                var simplePost = new 
                {
                    PostId = post.PostId,
                    UserId = post.UserId,
                    Title = post.Title,
                    Content = post.Content,
                    ImageUrl = post.ImageUrl,
                    CreatedAt = post.CreatedAt,
                    User = post.User != null ? new 
                    {
                        UserId = post.User.UserId,
                        FullName = post.User.FullName,
                        Email = post.User.Email
                    } : null
                };

                return Ok(simplePost);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy bài viết ID {id}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, $"Đã xảy ra lỗi khi lấy bài viết: {ex.Message}");
            }
        }

        // GET: api/Post/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Post>>> GetPostsByUserId(int userId)
        {
            try
            {
                Console.WriteLine($"Yêu cầu lấy bài viết của người dùng ID: {userId}");
                
                // Truy vấn tương tự như:
                // SELECT [PostId], [UserId], [Title], [Content], [ImageUrl], [CreatedAt] 
                // FROM [blog].[dbo].[Posts]
                // WHERE [UserId] = {userId}
                // ORDER BY [CreatedAt] DESC
                var posts = await _context.Posts
                    .Where(p => p.UserId == userId)
                    .Include(p => p.User)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                if (posts == null || !posts.Any())
                {
                    Console.WriteLine($"Không tìm thấy bài viết nào của người dùng ID: {userId}");
                    return NotFound("Không tìm thấy bài viết nào của người dùng này");
                }

                Console.WriteLine($"Đã tìm thấy {posts.Count} bài viết của người dùng ID: {userId}");
                
                // Chuyển đổi thành mảng JSON thuần túy với chính xác các trường từ database
                var simplePostsArray = posts.Select(p => new 
                {
                    PostId = p.PostId,
                    UserId = p.UserId,
                    Title = p.Title,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    User = p.User != null ? new 
                    {
                        UserId = p.User.UserId,
                        FullName = p.User.FullName,
                        Email = p.User.Email
                    } : null
                }).ToList();

                return Ok(simplePostsArray);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy bài viết của người dùng ID {userId}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, $"Đã xảy ra lỗi khi lấy bài viết của người dùng: {ex.Message}");
            }
        }

        // POST: api/Post
        [HttpPost]
        public async Task<ActionResult<Post>> CreatePost([FromForm] PostCreateViewModel model)
        {
            try
            {
                Console.WriteLine($"Received post creation request with Title: {model.Title}");
                
                // Tạo đối tượng Post mới
                var post = new Post
                {
                    Title = model.Title,
                    Content = model.Content,
                    UserId = model.UserId,
                    CreatedAt = DateTime.Now
                };

                // Upload ảnh lên Cloudinary nếu có
                if (model.Image != null && model.Image.Length > 0)
                {
                    try
                    {
                        Console.WriteLine($"Uploading image: {model.Image.FileName}, Size: {model.Image.Length} bytes");
                        var uploadResult = await UploadImageToCloudinary(model.Image);
                        
                        // Lưu URL ảnh từ Cloudinary vào cột ImageUrl
                        post.ImageUrl = uploadResult.SecureUrl.ToString();
                        Console.WriteLine($"Image uploaded successfully. URL: {post.ImageUrl}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error uploading image: {ex.Message}");
                        // Nếu lỗi upload ảnh, vẫn tiếp tục tạo bài viết nhưng không có ảnh
                    }
                }

                // Thêm bài viết vào database
                _context.Posts.Add(post);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"Post created successfully with ID: {post.PostId}");

                // Trả về thông tin bài viết vừa tạo, bao gồm cả URL ảnh
                var result = new
                {
                    post.PostId,
                    post.Title,
                    post.Content,
                    post.ImageUrl,
                    post.CreatedAt,
                    post.UserId
                };

                return CreatedAtAction(nameof(GetPost), new { id = post.PostId }, result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating post: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, $"Đã xảy ra lỗi khi tạo bài viết: {ex.Message}");
            }
        }
        
        // PUT: api/Post/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromForm] PostUpdateViewModel model)
        {
            if (id != model.PostId)
            {
                return BadRequest("ID không trùng khớp");
            }

            try
            {
                Console.WriteLine($"Received update request for post ID: {id}");
                
                var existingPost = await _context.Posts.FindAsync(id);
                if (existingPost == null)
                {
                    Console.WriteLine($"Post not found with ID: {id}");
                    return NotFound("Không tìm thấy bài viết để cập nhật");
                }

                // Lưu URL ảnh cũ để trả về nếu không có ảnh mới
                string oldImageUrl = existingPost.ImageUrl;

                // Cập nhật thông tin bài viết
                existingPost.Title = model.Title;
                existingPost.Content = model.Content;
                
                // Xử lý ảnh mới nếu có
                if (model.Image != null && model.Image.Length > 0)
                {
                    try
                    {
                        Console.WriteLine($"Uploading new image: {model.Image.FileName}, Size: {model.Image.Length} bytes");
                        var uploadResult = await UploadImageToCloudinary(model.Image);
                        
                        // Lưu URL ảnh mới từ Cloudinary
                        existingPost.ImageUrl = uploadResult.SecureUrl.ToString();
                        Console.WriteLine($"New image uploaded successfully. URL: {existingPost.ImageUrl}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error uploading new image: {ex.Message}");
                        // Nếu lỗi upload ảnh, vẫn tiếp tục cập nhật bài viết nhưng giữ ảnh cũ
                    }
                }
                // Không có trường ImageUrl nữa, chỉ xử lý ảnh qua trường Image
                
                _context.Entry(existingPost).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"Post updated successfully with ID: {id}");

                // Trả về kết quả với URL ảnh mới (hoặc giữ nguyên)
                var result = new
                {
                    existingPost.PostId,
                    existingPost.Title,
                    existingPost.Content,
                    existingPost.ImageUrl,
                    existingPost.CreatedAt,
                    existingPost.UserId,
                    Success = true
                };

                return Ok(result);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!PostExists(id))
                {
                    Console.WriteLine($"Post not found during concurrency check for ID: {id}");
                    return NotFound("Không tìm thấy bài viết");
                }
                else
                {
                    Console.WriteLine($"Concurrency exception updating post ID {id}: {ex.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating post ID {id}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, $"Đã xảy ra lỗi khi cập nhật bài viết: {ex.Message}");
            }
        }

        // DELETE: api/Post/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                var post = await _context.Posts.FindAsync(id);
                if (post == null)
                {
                    return NotFound("Không tìm thấy bài viết để xóa");
                }

                Console.WriteLine($"Deleting post with ID: {id}, ImageUrl: {post.ImageUrl}");

                // Xóa ảnh từ Cloudinary nếu có
                if (!string.IsNullOrEmpty(post.ImageUrl))
                {
                    try
                    {
                        // Trích xuất publicId từ URL
                        string publicId = null;
                        
                        // URL Cloudinary thường có dạng: https://res.cloudinary.com/cloudName/image/upload/v123456/folder/publicId
                        Uri uri = new Uri(post.ImageUrl);
                        string path = uri.AbsolutePath;
                        // Tìm phần sau "/upload/" trong path
                        int uploadIndex = path.IndexOf("/upload/");
                        if (uploadIndex >= 0)
                        {
                            string afterUpload = path.Substring(uploadIndex + 8); // +8 để bỏ qua "/upload/"
                            // Bỏ đi thông tin version nếu có (vXXXXXX/)
                            if (afterUpload.StartsWith("v") && afterUpload.Contains("/"))
                            {
                                afterUpload = afterUpload.Substring(afterUpload.IndexOf('/') + 1);
                            }
                            publicId = afterUpload; // Phần còn lại là publicId
                            
                            if (publicId.StartsWith("blog_posts/")) // Nếu đúng là ảnh từ thư mục blog_posts
                            {
                                Console.WriteLine($"Attempting to delete image with publicId: {publicId}");
                                await DeleteImageFromCloudinary(publicId);
                                Console.WriteLine($"Image deleted from Cloudinary: {publicId}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting image from Cloudinary: {ex.Message}");
                        // Tiếp tục xóa bài viết ngay cả khi xóa ảnh thất bại
                    }
                }

                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"Post with ID {id} deleted successfully");

                return Ok(new { Success = true, Message = "Bài viết đã được xóa thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting post with ID {id}: {ex.Message}");
                return StatusCode(500, $"Đã xảy ra lỗi khi xóa bài viết: {ex.Message}");
            }
        }

        // GET: api/Post/search?query=xxx
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Post>>> SearchPosts([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                {
                    return await GetPosts();
                }

                var posts = await _context.Posts
                    .Where(p => p.Title.Contains(query) || p.Content.Contains(query))
                    .Include(p => p.User)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                if (!posts.Any())
                {
                    return NotFound($"Không tìm thấy bài viết nào phù hợp với từ khóa '{query}'");
                }

                return posts;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi khi tìm kiếm bài viết: {ex.Message}");
            }
        }
        
        // POST: api/Post/upload-image
        [HttpPost("upload-image")]
        public async Task<ActionResult<ImageUploadResult>> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Không có tệp nào được tải lên");
                }

                var uploadResult = await UploadImageToCloudinary(file);

                return Ok(new { 
                    imageUrl = uploadResult.SecureUrl.ToString(),
                    publicId = uploadResult.PublicId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi khi tải ảnh lên: {ex.Message}");
            }
        }

        // Phương thức hỗ trợ upload ảnh lên Cloudinary
        private async Task<ImageUploadResult> UploadImageToCloudinary(IFormFile file)
        {
            try
            {
                Console.WriteLine($"Bắt đầu upload ảnh: {file.FileName}, Kích thước: {file.Length} bytes");
                
                using var stream = file.OpenReadStream();
                
                // Thêm timestamp để tránh trùng tên
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                string uniqueFileName = $"{fileName}_{timestamp}";
                
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "blog_posts",
                    PublicId = uniqueFileName, // Sử dụng tên file duy nhất
                    Transformation = new Transformation()
                        .Width(1200)
                        .Height(800)
                        .Crop("limit")
                        .Quality(90) // Chất lượng ảnh 90%
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                
                if (uploadResult.Error != null)
                {
                    Console.WriteLine($"Lỗi khi upload ảnh lên Cloudinary: {uploadResult.Error.Message}");
                    throw new Exception($"Lỗi Cloudinary: {uploadResult.Error.Message}");
                }
                
                Console.WriteLine($"Upload ảnh thành công. URL: {uploadResult.SecureUrl}, PublicId: {uploadResult.PublicId}");
                
                return uploadResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi upload ảnh: {ex.Message}");
                throw new Exception($"Không thể upload ảnh: {ex.Message}", ex);
            }
        }
        
        // Phương thức hỗ trợ xóa ảnh từ Cloudinary
        private async Task<DeletionResult> DeleteImageFromCloudinary(string publicId)
        {
            try
            {
                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);
                
                if (result.Error != null)
                {
                    Console.WriteLine($"Lỗi khi xóa ảnh từ Cloudinary: {result.Error.Message}");
                    // Không throw exception ở đây vì việc xóa ảnh có thể không quan trọng đến mức phải làm fail request
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa ảnh: {ex.Message}");
                // Không throw exception vì không muốn làm fail request chỉ vì không xóa được ảnh
                return new DeletionResult { Result = "error", Error = new Error { Message = ex.Message } };
            }
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.PostId == id);
        }
    }
    
    // ViewModel cho tạo bài viết
    public class PostCreateViewModel
    {
        /// <summary>
        /// Tiêu đề của bài viết
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Nội dung của bài viết
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// ID của người dùng tạo bài viết
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// File ảnh được tải lên từ máy tính
        /// </summary>
        public IFormFile Image { get; set; }
    }

    // ViewModel cho cập nhật bài viết
    public class PostUpdateViewModel
    {
        /// <summary>
        /// ID của bài viết cần cập nhật
        /// </summary>
        public int PostId { get; set; }

        /// <summary>
        /// Tiêu đề mới của bài viết
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Nội dung mới của bài viết
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// File ảnh mới được tải lên từ máy tính
        /// </summary>
        public IFormFile Image { get; set; }
    }
} 