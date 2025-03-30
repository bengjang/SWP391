using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using lamlai.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using System.Data;

namespace lamlai2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly TestContext _context;

        public AdminController(TestContext context)
        {
            _context = context;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product) // Load thông tin sản phẩm
                    .ToListAsync();

                var orderDtos = orders.Select(order => new
                {
                    order.OrderId,
                    order.UserId,
                    order.OrderDate,
                    order.OrderStatus,
                    order.DeliveryStatus,
                    order.DeliveryAddress,
                    order.Note,
                    order.VoucherId,
                    order.TotalAmount,
                    order.Name,
                    order.PhoneNumber,
                    Items = order.OrderItems.Select(oi => new
                    {
                        oi.Product.ProductName,
                        oi.Price,
                        oi.Quantity
                    }).ToList()
                }).ToList();

                return Ok(orderDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        // Thêm sản phẩm mới - Cho phép Staff thêm sản phẩm
        [HttpPost("Product")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductUpdateDTO productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kiểm tra xác thực thủ công bằng cách đọc header
            string userRole = null;
            if (Request.Headers.ContainsKey("User-Role"))
            {
                userRole = Request.Headers["User-Role"].ToString();
            }
            else if (Request.Headers.ContainsKey("X-User-Role"))
            {
                userRole = Request.Headers["X-User-Role"].ToString();
            }
            else if (Request.Headers.ContainsKey("Role"))
            {
                userRole = Request.Headers["Role"].ToString();
            }

            if (string.IsNullOrEmpty(userRole) || 
               (userRole != "Admin" && userRole != "Manager" && userRole != "Staff"))
            {
                return Unauthorized(new { error = "Bạn không có quyền thêm sản phẩm" });
            }

            try
            {
                // Tạo ProductCode thủ công thay vì dùng stored procedure
                string productPrefix = "SP";
                int nextProductNumber = 1;

                try
                {
                    // Tìm ProductCode lớn nhất trong hệ thống để tạo mã tiếp theo
                    var lastProductCode = await _context.Products
                        .OrderByDescending(p => p.ProductId)
                        .Select(p => p.ProductCode)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(lastProductCode) && lastProductCode.StartsWith(productPrefix))
                    {
                        string numericPart = lastProductCode.Substring(productPrefix.Length);
                        if (int.TryParse(numericPart, out int lastNumber))
                        {
                            nextProductNumber = lastNumber + 1;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Nếu có lỗi khi tìm mã sản phẩm, sử dụng mã mặc định
                    Console.WriteLine($"Lỗi khi tìm mã sản phẩm: {ex.Message}");
                }

                // Tạo mã sản phẩm mới với định dạng "SPxxx"
                string newProductCode = $"{productPrefix}{nextProductNumber:D3}";
                Console.WriteLine($"Mã sản phẩm mới: {newProductCode}");

                // Tạo sản phẩm với mã đã tạo
                var product = new Product
                {
                    ProductCode = newProductCode,
                    ProductName = productDto.ProductName,
                    CategoryId = productDto.CategoryId,
                    Quantity = productDto.Quantity,
                    Capacity = productDto.Capacity,
                    Price = productDto.Price,
                    Brand = productDto.Brand,
                    Origin = productDto.Origin,
                    Status = productDto.Status,
                    ImgUrl = productDto.ImgUrl,
                    SkinType = productDto.SkinType,
                    Description = productDto.Description,
                    Ingredients = productDto.Ingredients,
                    UsageInstructions = productDto.UsageInstructions,
                    ManufactureDate = productDto.ManufactureDate,
                    ImportDate = DateTime.Now // Tự động đặt ngày nhập kho
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetProductById", new { id = product.ProductId }, product);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { error = "Lỗi khi lưu dữ liệu vào database.", details = ex.InnerException?.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi không xác định.", details = ex.Message });
            }
        }

        // Thêm phương thức GetProductById để CreatedAtAction có thể hoạt động
        [HttpGet("{id}/product")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(new { error = "Không tìm thấy sản phẩm" });
            }

            return Ok(product);
        }

        [HttpPut("{id}/product")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpdateDTO productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Sản phẩm không tồn tại." });
            }

            try
            {
                // Cập nhật dữ liệu từ DTO
                product.ProductName = productDto.ProductName;
                product.CategoryId = productDto.CategoryId;
                product.Quantity = productDto.Quantity;
                product.Capacity = productDto.Capacity;
                product.Price = productDto.Price;
                product.Brand = productDto.Brand;
                product.Origin = productDto.Origin;
                product.Status = productDto.Status;
                product.ImgUrl = productDto.ImgUrl;
                product.SkinType = productDto.SkinType;
                product.Description = productDto.Description;
                product.Ingredients = productDto.Ingredients;
                product.UsageInstructions = productDto.UsageInstructions;
                product.ManufactureDate = productDto.ManufactureDate;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật sản phẩm thành công." });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { error = "Lỗi khi lưu dữ liệu vào database.", details = ex.InnerException?.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi không xác định.", details = ex.Message });
            }
        }

        public class ProductUpdateDTO
        {
            [Required]
            public string ProductName { get; set; } = null!;

            [Required]
            public int CategoryId { get; set; }

            [Required]
            public int Quantity { get; set; }

            [Required]
            public string Capacity { get; set; } = null!;

            [Required]
            [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0.")]
            public decimal Price { get; set; }

            public string Brand { get; set; } = null!;
            public string Origin { get; set; } = null!;
            public string Status { get; set; } = null!;
            public string ImgUrl { get; set; } = null!;
            public string SkinType { get; set; } = null!;
            public string Description { get; set; } = null!;
            public string Ingredients { get; set; } = null!;
            public string UsageInstructions { get; set; } = null!;
            public DateTime ManufactureDate { get; set; }
        }


        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleProductStatus(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { error = "Sản phẩm không tồn tại." });
            }

            // Đảo trạng thái giữa "Available" và "Unavailable"
            product.Status = (product.Status == "Available") ? "Out of Stock" : "Available";

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Trạng thái sản phẩm đã được cập nhật.", newStatus = product.Status });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { error = "Lỗi khi lưu dữ liệu vào database.", details = ex.InnerException?.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi không xác định.", details = ex.Message });
            }
        }

        [HttpPut("{id}/cancelrequest/approve")]
        public async Task<IActionResult> ApproveCancelRequest(int id)
        {
            var cancelRequest = await _context.CancelRequests.FindAsync(id);
            if (cancelRequest == null)
            {
                return NotFound(new { error = "Yêu cầu hủy không tồn tại." });
            }

            if (cancelRequest.Status != "Pending")
            {
                return BadRequest(new { error = "Yêu cầu hủy đã được xử lý trước đó." });
            }

            var order = await _context.Orders.FindAsync(cancelRequest.OrderId);
            if (order == null)
            {
                return NotFound(new { error = "Đơn hàng không tồn tại." });
            }

            // Cập nhật trạng thái đơn hàng
            order.OrderStatus = "Cancelled";
            cancelRequest.Status = "Approved";

            await _context.SaveChangesAsync();
            return Ok(new { message = "Yêu cầu hủy đã được duyệt và đơn hàng đã bị hủy." });
        }
        [HttpPut("{id}/cancelrequest/reject")]
        public async Task<IActionResult> RejectCancelRequest(int id)
        {
            var cancelRequest = await _context.CancelRequests.FindAsync(id);
            if (cancelRequest == null)
            {
                return NotFound(new { error = "Yêu cầu hủy không tồn tại." });
            }

            if (cancelRequest.Status != "Pending")
            {
                return BadRequest(new { error = "Yêu cầu hủy đã được xử lý trước đó." });
            }

            cancelRequest.Status = "Rejected";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Yêu cầu hủy đã bị từ chối." });
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<CancelRequest>>> GetCancelRequestsByStatus(string status)
        {
            var validStatuses = new List<string> { "Pending", "Approved", "Rejected" };
            if (!validStatuses.Contains(status))
            {
                return BadRequest(new { error = "Trạng thái không hợp lệ." });
            }

            var cancelRequests = await _context.CancelRequests
                .Where(cr => cr.Status == status)
                .ToListAsync();

            return Ok(cancelRequests);
        }


    }
}



