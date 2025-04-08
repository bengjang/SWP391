using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using lamlai.Models;
using System.Globalization; // Thêm để chuẩn hóa chuỗi
using System.Text; // Thêm để chuẩn hóa chuỗi
using SWP391.Services; // Thêm để sử dụng IPhotoService
using CloudinaryDotNet.Actions; // Thêm để sử dụng ImageUploadResult
using System.IO; // Thêm để sử dụng Path.GetExtension

namespace test2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkincareRoutinesController : ControllerBase
    {
        private readonly TestContext _context;
        private readonly IPhotoService _photoService;

        public SkincareRoutinesController(TestContext context, IPhotoService photoService)
        {
            _context = context;
            _photoService = photoService;
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

                string normalizedSkinType = skinType.ToLower();
                Console.WriteLine($"[Backend Log - GET] Received skinType: {skinType}");
                Console.WriteLine($"[Backend Log - GET] Normalized skinType: {normalizedSkinType}");

                // Find routine logic (use the same logic as before to find the correct routine)
                var routine = await FindRoutineByNormalizedSkinTypeAsync(normalizedSkinType); // Use helper method

                if (routine == null)
                {
                    return NotFound($"Không tìm thấy quy trình chăm sóc da cho loại da {skinType}");
                }

                return Ok(routine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Backend Error - GET] Error getting routine for skin type {skinType}: {ex.ToString()}");
                return StatusCode(500, $"Lỗi khi lấy quy trình chăm sóc da theo loại da: {ex.Message}");
            }
        }

        // PUT: api/SkincareRoutines/skintype/{skinType}/content
        [HttpPut("skintype/{skinType}/content")]
        public async Task<IActionResult> UpdateRoutineContentBySkinType(string skinType, [FromBody] SkincareRoutineUpdateRequest request)
        {
            if (request == null)
            {
                return BadRequest("Dữ liệu cập nhật không hợp lệ.");
            }

            try
            {
                // Chuẩn hóa chuỗi đầu vào
                skinType = skinType?.Trim();
                
                if (string.IsNullOrEmpty(skinType))
                {
                    return BadRequest("Loại da không được để trống");
                }

                string normalizedSkinType = skinType.ToLower();
                Console.WriteLine($"[Backend Log - PUT] Received skinType: {skinType}");
                Console.WriteLine($"[Backend Log - PUT] Normalized skinType: {normalizedSkinType}");

                // Tìm quy trình hiện có
                var routine = await FindRoutineByNormalizedSkinTypeAsync(normalizedSkinType);

                if (routine == null)
                {
                    return NotFound($"Không tìm thấy quy trình chăm sóc da cho loại da {skinType} để cập nhật.");
                }

                Console.WriteLine($"[Backend Log - PUT] Found routine to update: ID {routine.RoutineId}, Title: {routine.Title}");

                // Cập nhật các trường từ request, giữ nguyên UserId và CreatedAt
                if (!string.IsNullOrEmpty(request.Title))
                {
                    routine.Title = request.Title;
                }
                if (!string.IsNullOrEmpty(request.Content))
                {
                    routine.Content = request.Content;
                }
                if (!string.IsNullOrEmpty(request.ImageUrl))
                {
                    routine.ImageUrl = request.ImageUrl;
                }

                // Đánh dấu entity đã thay đổi
                _context.Entry(routine).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"[Backend Log - PUT] Successfully updated routine for skin type: {skinType}");
                    return Ok(new { message = "Cập nhật nội dung thành công", routine });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.SkincareRoutines.AnyAsync(r => r.RoutineId == routine.RoutineId))
                    {
                        return NotFound($"Quy trình chăm sóc da không còn tồn tại.");
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Backend Error - PUT] Error updating routine for skin type {skinType}: {ex.ToString()}");
                return StatusCode(500, $"Lỗi máy chủ nội bộ khi cập nhật quy trình: {ex.Message}");
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

                // Tìm quy trình chăm sóc da phù hợp với loại da
                var routine = await _context.SkincareRoutines
                    .FirstOrDefaultAsync(r => r.SkinType.ToLower() == skinType.ToLower());

                if (routine == null)
                {
                    return NotFound($"Không tìm thấy quy trình chăm sóc da cho loại da {skinType}");
                }

                // Lấy các sản phẩm được gán cho quy trình này từ bảng SkincareRoutineProducts
                var routineProducts = await _context.SkincareRoutineProducts
                    .Where(rp => rp.RoutineId == routine.RoutineId)
                    .OrderBy(rp => rp.OrderIndex)
                    .ToListAsync();

                if (routineProducts == null || !routineProducts.Any())
                {
                    return NotFound($"Không tìm thấy sản phẩm đề xuất cho quy trình chăm sóc da {skinType}");
                }

                // Lấy thông tin chi tiết của từng sản phẩm
                var productDetails = new List<object>();
                foreach (var routineProduct in routineProducts)
                {
                    var product = await _context.Products
                        .Where(p => p.ProductId == routineProduct.ProductID && p.Status == "Active")
                        .FirstOrDefaultAsync();

                    if (product != null)
                    {
                        // Lấy hình ảnh sản phẩm
                        var productImage = await _context.ProductImages
                            .Where(pi => pi.ProductId == product.ProductId)
                            .Select(pi => pi.ImgUrl)
                            .FirstOrDefaultAsync();

                        productDetails.Add(new
                        {
                            id = product.ProductId,
                            name = product.ProductName,
                            price = product.Price,
                            brand = product.Brand,
                            imageUrl = productImage ?? "",
                            stepName = routineProduct.StepName,
                            orderIndex = routineProduct.OrderIndex,
                            customDescription = routineProduct.CustomDescription
                        });
                    }
                }

                if (productDetails.Count == 0)
                {
                    return NotFound($"Không tìm thấy thông tin chi tiết sản phẩm cho quy trình chăm sóc da {skinType}");
                }

                return Ok(productDetails);
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
            if (id != routine.RoutineId)
            {
                return BadRequest(new { message = "ID không khớp" });
            }

            try
            {
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

                return Ok(new { message = "Cập nhật quy trình thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật quy trình", error = ex.Message });
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
                    return NotFound(new { message = "Không tìm thấy quy trình" });
                }

                _context.SkincareRoutines.Remove(routine);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Xóa quy trình thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa quy trình", error = ex.Message });
            }
        }

        // PUT: api/SkincareRoutines/skintype/{skinType}/products
        [HttpPut("skintype/{skinType}/products")]
        public async Task<IActionResult> UpdateRoutineProducts(string skinType, [FromBody] List<SkincareRoutineProduct> products)
        {
            try
            {
                // Tìm routine theo skinType
                var routine = await _context.SkincareRoutines
                    .FirstOrDefaultAsync(r => r.SkinType.ToLower() == skinType.ToLower());

                if (routine == null)
                {
                    return NotFound($"Không tìm thấy quy trình cho loại da {skinType}");
                }

                // Xóa tất cả sản phẩm cũ của routine
                var existingProducts = await _context.SkincareRoutineProducts
                    .Where(p => p.RoutineId == routine.RoutineId)
                    .ToListAsync();
                _context.SkincareRoutineProducts.RemoveRange(existingProducts);

                // Thêm sản phẩm mới
                foreach (var product in products)
                {
                    product.RoutineId = routine.RoutineId;
                    _context.SkincareRoutineProducts.Add(product);
                }

                await _context.SaveChangesAsync();
                return Ok("Cập nhật sản phẩm thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi cập nhật sản phẩm: {ex.Message}");
            }
        }

        // PUT: api/SkincareRoutines/content/{skinType}
        [HttpPut("content/{skinType}")]
        public async Task<IActionResult> UpdateRoutineContent(string skinType, [FromBody] SkincareRoutine content)
        {
            try
            {
                var existingContent = await _context.SkincareRoutines
                    .FirstOrDefaultAsync(r => r.SkinType == skinType);

                if (existingContent == null)
                {
                    content.SkinType = skinType;
                    _context.SkincareRoutines.Add(content);
                }
                else
                {
                    existingContent.Title = content.Title;
                    existingContent.Content = content.Content;
                    existingContent.ImageUrl = content.ImageUrl;
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Cập nhật nội dung quy trình thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật nội dung quy trình", error = ex.Message });
            }
        }

        // GET: api/SkincareRoutines/content/{skinType}
        [HttpGet("content/{skinType}")]
        public async Task<IActionResult> GetRoutineContent(string skinType)
        {
            try
            {
                var content = await _context.SkincareRoutines
                    .FirstOrDefaultAsync(r => r.SkinType == skinType);

                if (content == null)
                {
                    return NotFound(new { message = "Không tìm thấy nội dung quy trình cho loại da này" });
                }

                return Ok(content);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy nội dung quy trình", error = ex.Message });
            }
        }

        // PUT: api/SkincareRoutines/skintype/{skinType}/upload-image
        [HttpPost("skintype/{skinType}/upload-image")]
        public async Task<IActionResult> UploadRoutineImage(string skinType, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Không có file ảnh được gửi lên.");
            }

            // Kiểm tra loại file
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest($"Loại file không hợp lệ. Chỉ chấp nhận: {string.Join(", ", allowedExtensions)}");
            }

            try
            {
                // Chuẩn hóa chuỗi đầu vào
                skinType = skinType?.Trim();
                
                if (string.IsNullOrEmpty(skinType))
                {
                    return BadRequest("Loại da không được để trống");
                }

                string normalizedSkinType = skinType.ToLower();
                Console.WriteLine($"[Backend Log - POST] Received skinType for image upload: {skinType}");
                Console.WriteLine($"[Backend Log - POST] Normalized skinType: {normalizedSkinType}");

                // Tìm quy trình hiện có
                var routine = await FindRoutineByNormalizedSkinTypeAsync(normalizedSkinType);

                if (routine == null)
                {
                    return NotFound($"Không tìm thấy quy trình chăm sóc da cho loại da {skinType} để cập nhật ảnh.");
                }

                Console.WriteLine($"[Backend Log - POST] Found routine to update image: ID {routine.RoutineId}, Title: {routine.Title}");

                // Sử dụng dịch vụ upload ảnh
                var result = await _photoService.AddPhotoAsync(file);

                if (result.Error != null)
                {
                    return BadRequest($"Lỗi khi tải lên ảnh: {result.Error.Message}");
                }

                // Cập nhật URL ảnh trong cơ sở dữ liệu
                routine.ImageUrl = result.SecureUrl.AbsoluteUri;

                // Đánh dấu entity đã thay đổi
                _context.Entry(routine).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"[Backend Log - POST] Successfully updated image for routine (skin type: {skinType})");
                    Console.WriteLine($"[Backend Log - POST] Image URL: {routine.ImageUrl}");
                    
                    // Trả về URL ảnh đã cập nhật
                    var response = new {
                        message = "Cập nhật ảnh thành công",
                        imageUrl = routine.ImageUrl,
                        success = true,
                        routineId = routine.RoutineId
                    };
                    
                    Console.WriteLine($"[Backend Log - POST] Returning response: {System.Text.Json.JsonSerializer.Serialize(response)}");
                    return Ok(response);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Console.WriteLine($"[Backend Error - POST] Concurrency error updating image: {ex.ToString()}");
                    return StatusCode(500, $"Lỗi đồng thởi khi cập nhật ảnh: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Backend Error - POST] Error updating image: {ex.ToString()}");
                    return StatusCode(500, $"Lỗi khi cập nhật ảnh: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Backend Error - POST] Error in upload image process: {ex.ToString()}");
                return StatusCode(500, $"Lỗi trong quá trình tải lên ảnh: {ex.Message}");
            }
        }

        // Helper method to find routine by normalized skin type (Extracted for reuse)
        // Consider making this private if only used within this controller
        private async Task<SkincareRoutine> FindRoutineByNormalizedSkinTypeAsync(string normalizedSkinType)
        {
            Console.WriteLine($"[Backend Log - Helper] Finding routine for normalized skinType: {normalizedSkinType}");
            // Ánh xạ các tên không dấu với tên có dấu
            var skinTypeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "dadau", "Da dầu" },
                { "dakho", "Da khô" },
                { "dathuong", "Da thường" },
                { "dahonhop", "Da hỗn hợp" },
                { "danhaycam", "Da nhạy cảm" }
            };

            string mappedSkinType = null;
            if (skinTypeMap.TryGetValue(normalizedSkinType, out string value))
            {
                mappedSkinType = value;
            }
            Console.WriteLine($"[Backend Log - Helper] Mapped skinType: {mappedSkinType}");

            // Tìm kiếm chính xác trước (theo tên gốc đã chuẩn hóa)
            var routine = await _context.SkincareRoutines
                .FirstOrDefaultAsync(r => r.SkinType.ToLower() == normalizedSkinType);
            Console.WriteLine($"[Backend Log - Helper] Routine found by normalized name: {(routine != null ? routine.RoutineId.ToString() : "NULL")}");

            // Nếu không tìm thấy, thử tìm bằng tên đã ánh xạ
            if (routine == null && mappedSkinType != null)
            {
                routine = await _context.SkincareRoutines
                    .FirstOrDefaultAsync(r => r.SkinType.ToLower() == mappedSkinType.ToLower());
                Console.WriteLine($"[Backend Log - Helper] Routine found by mapped name: {(routine != null ? routine.RoutineId.ToString() : "NULL")}");
            }

            // Nếu vẫn không tìm thấy, thử tìm kiếm bằng Contains (cả tên gốc và tên ánh xạ nếu có)
            // Ưu tiên tìm Contains trên tên gốc trước
            if (routine == null)
            {
                routine = await _context.SkincareRoutines
                    .FirstOrDefaultAsync(r => r.SkinType.ToLower().Contains(normalizedSkinType) || 
                                            normalizedSkinType.Contains(r.SkinType.ToLower()));
                Console.WriteLine($"[Backend Log - Helper] Routine found by Contains (normalized): {(routine != null ? routine.RoutineId.ToString() : "NULL")}");
            }
            // Nếu vẫn không tìm thấy và có tên ánh xạ, thử Contains trên tên ánh xạ
            if (routine == null && mappedSkinType != null)
            {
                routine = await _context.SkincareRoutines
                    .FirstOrDefaultAsync(r => r.SkinType.ToLower().Contains(mappedSkinType.ToLower()) || 
                                             mappedSkinType.ToLower().Contains(r.SkinType.ToLower()));
                Console.WriteLine($"[Backend Log - Helper] Routine found by Contains (mapped): {(routine != null ? routine.RoutineId.ToString() : "NULL")}");
            }

            // Bỏ phần tìm kiếm bằng map đặc biệt vì nó trùng lặp logic ánh xạ ở trên.

            return routine;
        }
    }

    // *** NEW DTO CLASS ***
    public class SkincareRoutineUpdateRequest
    {
        public string? Title { get; set; } // Cho phép null để linh hoạt hơn
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class SkincareRoutineProductUpdateDto
    {

    }
}
