//using Microsoft.AspNetCore.Mvc;
//using lamlai.Models;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;

//namespace lamlai.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class BlogController : ControllerBase
//    {
//        private readonly TestContext _context;

//        public BlogController(TestContext context)
//        {
//            _context = context;
//        }

//        // GET: api/post
//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<PostDto>>> GetPosts()
//        {
//            var posts = await _context.Posts
//                .Select(post => new PostDto
//                {
//                    Title = post.Title,
//                    Content = post.Content,
//                    ImageUrl = post.ImageUrl
//                })
//                .ToListAsync();

//            return posts;
//        }

//        // GET: api/post/{id}
//        [HttpGet("{id}")]
//        public async Task<ActionResult<PostDto>> GetPost(int id)
//        {
//            var post = await _context.Posts
//                .Where(p => p.PostId == id)
//                .Select(p => new PostDto
//                {
//                    Title = p.Title,
//                    Content = p.Content,
//                    ImageUrl = p.ImageUrl
//                })
//                .FirstOrDefaultAsync();

//            if (post == null)
//            {
//                return NotFound();
//            }

//            return post;
//        }

//        [HttpPost]
//        public async Task<ActionResult<Post>> CreatePost(PostDto postDto)
//        {
//            var post = new Post
//            {
//                Title = postDto.Title,
//                Content = postDto.Content,
//                ImageUrl = postDto.ImageUrl,
//                UserId = postDto.UserId // Cần UserId để lưu vào database
//            };

//            _context.Posts.Add(post);
//            await _context.SaveChangesAsync();

//            return CreatedAtAction(nameof(GetPost), new { id = post.PostId }, post);
//        }

//        // PUT: api/post/{id}
//        [HttpPut("{id}")]
//        public async Task<IActionResult> UpdatePost(int id, PostDto postDto)
//        {
//            var post = await _context.Posts.FindAsync(id);
//            if (post == null)
//            {
//                return NotFound();
//            }

//            post.Title = postDto.Title;
//            post.Content = postDto.Content;
//            post.ImageUrl = postDto.ImageUrl;
//            // Cập nhật UserId nếu cần

//            _context.Entry(post).State = EntityState.Modified;

//            try
//            {
//                await _context.SaveChangesAsync();
//            }
//            catch (DbUpdateConcurrencyException)
//            {
//                if (!PostExists(id))
//                {
//                    return NotFound();
//                }
//                else
//                {
//                    throw;
//                }
//            }

//            return NoContent();
//        }

//        // DELETE: api/post/{id}
//        [HttpDelete("{id}")]
//        public async Task<IActionResult> DeletePost(int id)
//        {
//            var post = await _context.Posts.FindAsync(id);
//            if (post == null)
//            {
//                return NotFound();
//            }

//            _context.Posts.Remove(post);
//            await _context.SaveChangesAsync();

//            return NoContent();
//        }

//        private bool PostExists(int id)
//        {
//            return _context.Posts.Any(e => e.PostId == id);
//        }
//        public class PostDto
//        {

//            public int UserId { get; set; } // ID người dùng (người viết bài)
//            public string Title { get; set; } = null!; // Tiêu đề bài viết
//            public string Content { get; set; } = null!; // Nội dung bài viết
//            public string? ImageUrl { get; set; } // Đường dẫn hình ảnh (có thể null)
//            public DateTime CreatedAt { get; set; } = DateTime.Now; // Thời gian tạo bài viết
//        }
//    }
//}