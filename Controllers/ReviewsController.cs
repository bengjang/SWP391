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
    public class ReviewsController : ControllerBase
    {
        private readonly TestContext _context;

        public ReviewsController(TestContext context)
        {
            _context = context;
        }


        // ✅ 1. Lấy danh sách review theo sản phẩm
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviewsByProduct(int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .ToListAsync();

            if (!reviews.Any())
                return NotFound("Chưa có đánh giá nào cho sản phẩm này.");

            return Ok(reviews);
        }

        // ✅ 2. Lấy danh sách review theo người dùng
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviewsByUser(int userId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.UserId == userId)
                .ToListAsync();

            if (!reviews.Any())
                return NotFound("Người dùng chưa viết đánh giá nào.");

            return Ok(reviews);
        }

        [HttpPost]
        public async Task<ActionResult<Review>> PostReview([FromBody] ReviewDto reviewDto)
        {
            // Kiểm tra xem người dùng đã mua sản phẩm này chưa
            var hasPurchased = await _context.OrderItems
                .Include(oi => oi.Order)
                .AnyAsync(oi => oi.ProductId == reviewDto.ProductId &&
                                oi.Order.UserId == reviewDto.UserId &&
                                oi.Order.OrderStatus == "Completed"); // Chỉ tính đơn đã hoàn thành

            if (!hasPurchased)
                return BadRequest("Bạn chỉ có thể đánh giá sản phẩm sau khi đã mua.");

            // Tạo đối tượng Review từ dữ liệu đầu vào
            var review = new Review
            {
                UserId = reviewDto.UserId,
                ProductId = reviewDto.ProductId,
                Rating = reviewDto.Rating,
                ReviewDate = DateTime.UtcNow, // Tự động lấy thời gian hiện tại
                ReviewComment = reviewDto.ReviewComment
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReview), new { id = review.ReviewId }, review);
        }

        // DTO để nhận dữ liệu từ request
        public class ReviewDto
        {
            public int UserId { get; set; }
            public int ProductId { get; set; }
            public int Rating { get; set; }
            public string? ReviewComment { get; set; }
        }


        // ✅ 4. Thống kê điểm trung bình rating của sản phẩm
        [HttpGet("product/{productId}/average-rating")]
        public async Task<ActionResult<object>> GetAverageRating(int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .ToListAsync();

            if (!reviews.Any())
                return Ok(new { ProductId = productId, AverageRating = 0, TotalReviews = 0 });

            var averageRating = reviews.Average(r => r.Rating);

            return Ok(new
            {
                ProductId = productId,
                AverageRating = Math.Round(averageRating, 2), // Làm tròn 2 số thập phân
                TotalReviews = reviews.Count
            });
        }

        // GET: api/Reviews
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviews()
        {
            return await _context.Reviews.ToListAsync();
        }

        // GET: api/Reviews/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Review>> GetReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
            {
                return NotFound();
            }

            return review;
        }

        // PUT: api/Reviews/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReview(int id, Review review)
        {
            if (id != review.ReviewId)
            {
                return BadRequest();
            }

            _context.Entry(review).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReviewExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Reviews
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
   
        // DELETE: api/Reviews/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.ReviewId == id);
        }
    }
}
