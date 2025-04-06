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
    public class SkincareRoutinesController : ControllerBase
    {
        private readonly TestContext _context;

        public SkincareRoutinesController(TestContext context)
        {
            _context = context;
        }

        // GET: api/SkincareRoutines
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SkincareRoutine>>> GetAllRoutines()
        {
            try
            {
                var routines = await _context.SkincareRoutines.ToListAsync();
                return Ok(routines);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy danh sách quy trình chăm sóc da: {ex.Message}");
            }
        }

        // GET: api/SkincareRoutines/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SkincareRoutine>> GetRoutineById(int id)
        {
            try
            {
                var routine = await _context.SkincareRoutines.FindAsync(id);

                if (routine == null)
                {
                    return NotFound($"Không tìm thấy quy trình chăm sóc da với ID {id}");
                }

                return Ok(routine);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy quy trình chăm sóc da: {ex.Message}");
            }
        }

        // GET: api/SkincareRoutines/skintype/{skinType}
        [HttpGet("skintype/{skinType}")]
        public async Task<ActionResult<SkincareRoutine>> GetRoutineBySkinType(string skinType)
        {
            try
            {
                // Chuẩn hóa chuỗi đầu vào
                skinType = skinType?.Trim();
                
                if (string.IsNullOrEmpty(skinType))
                {
                    return BadRequest("Loại da không được để trống");
                }

                // Tìm kiếm chính xác trước
                var routine = await _context.SkincareRoutines
                    .FirstOrDefaultAsync(r => r.SkinType.ToLower() == skinType.ToLower());

                // Nếu không tìm thấy, thử tìm kiếm bằng Contains
                if (routine == null)
                {
                    routine = await _context.SkincareRoutines
                        .FirstOrDefaultAsync(r => r.SkinType.ToLower().Contains(skinType.ToLower()) || 
                                               skinType.ToLower().Contains(r.SkinType.ToLower()));
                }

                // Xử lý các trường hợp đặc biệt
                if (routine == null)
                {
                    // Ánh xạ các tên không dấu với tên có dấu
                    var skinTypeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "dadau", "Da dầu" },
                        { "dakho", "Da khô" },
                        { "dathuong", "Da thường" },
                        { "dahonhop", "Da hỗn hợp" },
                        { "danhaycam", "Da nhạy cảm" }
                    };

                    // Nếu skinType là một trong các key không dấu
                    if (skinTypeMap.TryGetValue(skinType.ToLower(), out string mappedSkinType))
                    {
                        routine = await _context.SkincareRoutines
                            .FirstOrDefaultAsync(r => r.SkinType.ToLower() == mappedSkinType.ToLower());
                    }
                }

                if (routine == null)
                {
                    return NotFound($"Không tìm thấy quy trình chăm sóc da cho loại da {skinType}");
                }

                return Ok(routine);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy quy trình chăm sóc da theo loại da: {ex.Message}");
            }
        }

        // GET: api/SkincareRoutines/skintype/{skinType}/products
        [HttpGet("skintype/{skinType}/products")]
        public async Task<ActionResult> GetProductsBySkinType(string skinType)
        {
            try
            {
                // Chuẩn hóa chuỗi đầu vào
                skinType = skinType?.Trim();
                
                if (string.IsNullOrEmpty(skinType))
                {
                    return BadRequest("Loại da không được để trống");
                }

                // Ánh xạ các tên không dấu với tên có dấu
                var skinTypeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "dadau", "Da dầu" },
                    { "dakho", "Da khô" },
                    { "dathuong", "Da thường" },
                    { "dahonhop", "Da hỗn hợp" },
                    { "danhaycam", "Da nhạy cảm" }
                };

                // Chuẩn hóa skinType nếu cần
                if (skinTypeMap.TryGetValue(skinType.ToLower(), out string mappedSkinType))
                {
                    skinType = mappedSkinType;
                }

                // Tìm các sản phẩm phù hợp cho loại da
                var products = await _context.Products
                    .Where(p => p.IsActive && 
                               (p.SuitableSkinTypes.Contains(skinType) || 
                                string.IsNullOrEmpty(p.SuitableSkinTypes) || 
                                p.SuitableSkinTypes.Contains("Tất cả loại da")))
                    .Select(p => new {
                        id = p.ProductId,
                        name = p.ProductName,
                        price = p.Price,
                        brand = p.Brand,
                        image = p.ProductImage,
                        category = p.CategoryName,
                        productType = p.ProductType,
                        stepName = MapProductTypeToStepName(p.ProductType) // Hàm helper để map loại sản phẩm sang bước chăm sóc da
                    })
                    .ToListAsync();

                if (products == null || !products.Any())
                {
                    return NotFound($"Không tìm thấy sản phẩm đề xuất cho loại da {skinType}");
                }

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy sản phẩm đề xuất theo loại da: {ex.Message}");
            }
        }

        // Hàm helper để map loại sản phẩm sang bước chăm sóc da
        private string MapProductTypeToStepName(string productType)
        {
            if (string.IsNullOrEmpty(productType))
                return null;

            var productTypeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Sữa rửa mặt", "Sữa rửa mặt" },
                { "Cleanser", "Sữa rửa mặt" },
                { "Face Wash", "Sữa rửa mặt" },
                { "Nước hoa hồng", "Toner" },
                { "Toner", "Toner" },
                { "Serum", "Serum" },
                { "Tinh chất", "Serum" },
                { "Essence", "Serum" },
                { "Kem dưỡng", "Kem dưỡng ẩm" },
                { "Moisturizer", "Kem dưỡng ẩm" },
                { "Cream", "Kem dưỡng ẩm" },
                { "Lotion", "Kem dưỡng ẩm" },
                { "Kem chống nắng", "Kem chống nắng" },
                { "Sunscreen", "Kem chống nắng" },
                { "Kem mắt", "Kem mắt" },
                { "Eye Cream", "Kem mắt" },
                { "Tẩy trang", "Tẩy trang" },
                { "Makeup Remover", "Tẩy trang" },
                { "Tẩy da chết", "Tẩy tế bào chết" },
                { "Exfoliator", "Tẩy tế bào chết" },
                { "Peeling", "Tẩy tế bào chết" },
                { "Scrub", "Tẩy tế bào chết" },
                { "Mặt nạ", "Đặc trị" },
                { "Mask", "Đặc trị" },
                { "Ampoule", "Đặc trị" },
                { "Trị mụn", "Đặc trị" }
            };

            // Kiểm tra từng từ khóa trong tên sản phẩm
            foreach (var key in productTypeMap.Keys)
            {
                if (productType.Contains(key, StringComparison.OrdinalIgnoreCase))
                {
                    return productTypeMap[key];
                }
            }

            // Mặc định trả về chính productType
            return productType;
        }

        // GET: api/SkincareRoutines/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<SkincareRoutine>>> GetRoutinesByUserId(int userId)
        {
            try
            {
                var routines = await _context.SkincareRoutines
                    .Where(r => r.UserId == userId)
                    .ToListAsync();

                if (routines == null || !routines.Any())
                {
                    return NotFound($"Không tìm thấy quy trình chăm sóc da cho người dùng với ID {userId}");
                }

                return Ok(routines);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy quy trình chăm sóc da theo người dùng: {ex.Message}");
            }
        }

        // POST: api/SkincareRoutines
        [HttpPost]
        public async Task<ActionResult<SkincareRoutine>> CreateRoutine(SkincareRoutine routine)
        {
            try
            {
                if (routine == null)
                {
                    return BadRequest("Dữ liệu quy trình chăm sóc da không hợp lệ");
                }

                // Đặt thời gian tạo
                routine.CreatedAt = DateTime.Now;

                _context.SkincareRoutines.Add(routine);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetRoutineById), new { id = routine.RoutineId }, routine);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi tạo quy trình chăm sóc da: {ex.Message}");
            }
        }

        // PUT: api/SkincareRoutines/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoutine(int id, SkincareRoutine routine)
        {
            try
            {
                if (id != routine.RoutineId)
                {
                    return BadRequest("ID không khớp");
                }

                var existingRoutine = await _context.SkincareRoutines.FindAsync(id);
                if (existingRoutine == null)
                {
                    return NotFound($"Không tìm thấy quy trình chăm sóc da với ID {id}");
                }

                // Cập nhật các trường
                existingRoutine.SkinType = routine.SkinType;
                existingRoutine.Title = routine.Title;
                existingRoutine.Content = routine.Content;
                existingRoutine.ImageUrl = routine.ImageUrl;
                // Không cập nhật UserId và CreatedAt

                _context.Entry(existingRoutine).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi cập nhật quy trình chăm sóc da: {ex.Message}");
            }
        }

        // DELETE: api/SkincareRoutines/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoutine(int id)
        {
            try
            {
                var routine = await _context.SkincareRoutines.FindAsync(id);
                if (routine == null)
                {
                    return NotFound($"Không tìm thấy quy trình chăm sóc da với ID {id}");
                }

                _context.SkincareRoutines.Remove(routine);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi xóa quy trình chăm sóc da: {ex.Message}");
            }
        }
    }
}
