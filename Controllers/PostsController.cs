using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using lamlai.Models;

namespace test2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly TestContext _context;

        public PostsController(TestContext context)
        {
            _context = context;
        }

        // GET: api/Posts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
        {
            try
            {
                return await _context.Posts
                    .Include(p => p.User)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi khi lấy danh sách bài viết: {ex.Message}");
            }
        }

        // GET: api/Posts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(int id)
        {
            try
            {
                var post = await _context.Posts
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.PostId == id);

                if (post == null)
                {
                    return NotFound("Không tìm thấy bài viết");
                }

                return post;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi khi lấy bài viết: {ex.Message}");
            }
        }

        // GET: api/Posts/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Post>>> GetPostsByUserId(int userId)
        {
            try
            {
                var posts = await _context.Posts
                    .Where(p => p.UserId == userId)
                    .Include(p => p.User)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                if (posts == null || !posts.Any())
                {
                    return NotFound("Không tìm thấy bài viết nào của người dùng này");
                }

                return posts;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi khi lấy bài viết của người dùng: {ex.Message}");
            }
        }

        // POST: api/Posts
        [HttpPost]
        public async Task<ActionResult<Post>> CreatePost(Post post)
        {
            try
            {
                post.CreatedAt = DateTime.Now;
                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPost), new { id = post.PostId }, post);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi khi tạo bài viết: {ex.Message}");
            }
        }

        // PUT: api/Posts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, Post post)
        {
            if (id != post.PostId)
            {
                return BadRequest("ID không trùng khớp");
            }

            try
            {
                var existingPost = await _context.Posts.FindAsync(id);
                if (existingPost == null)
                {
                    return NotFound("Không tìm thấy bài viết để cập nhật");
                }

                // Cập nhật thông tin bài viết
                existingPost.Title = post.Title;
                existingPost.Content = post.Content;
                if (!string.IsNullOrEmpty(post.ImageUrl))
                {
                    existingPost.ImageUrl = post.ImageUrl;
                }
                
                _context.Entry(existingPost).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(id))
                {
                    return NotFound("Không tìm thấy bài viết");
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi khi cập nhật bài viết: {ex.Message}");
            }
        }

        // DELETE: api/Posts/5
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

                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi khi xóa bài viết: {ex.Message}");
            }
        }

        // GET: api/Posts/search?query=xxx
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

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.PostId == id);
        }
    }
} 