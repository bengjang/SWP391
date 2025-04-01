using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Globalization;

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
                // Lấy thông tin danh mục để xác định tiền tố phù hợp
                var category = await _context.Categories.FindAsync(productDto.CategoryId);
                if (category == null)
                {
                    return BadRequest(new { error = "Danh mục không tồn tại" });
                }

                // Xác định tiền tố dựa trên loại danh mục
                string productPrefix;
                switch (category.CategoryType)
                {
                    case "Làm Sạch Da":
                        productPrefix = "LSD";
                        break;
                    case "Đặc Trị":
                        productPrefix = "ĐT";
                        break;
                    case "Dưỡng Ẩm":
                        productPrefix = "DA";
                        break;
                    case "Bộ Chăm Sóc Da Mặt":
                        productPrefix = "BCSDM";
                        break;
                    case "Chống Nắng Da Mặt":
                        productPrefix = "CNDM";
                        break;
                    case "Dưỡng Mắt":
                        productPrefix = "DM";
                        break;
                    case "Dụng Cụ/Phụ Kiện Chăm Sóc Da":
                        productPrefix = "PKCSD";
                        break;
                    case "Vấn Đề Về Da":
                        productPrefix = "VDVD";
                        break;
                    case "Dưỡng Môi":
                        productPrefix = "DMI";
                        break;
                    case "Mặt Nạ":
                        productPrefix = "MN";
                        break;
                    default:
                        // Mặc định, tạo tiền tố từ các chữ cái đầu của categoryType
                        productPrefix = string.Join("", category.CategoryType.Split(' ')
                            .Where(s => !string.IsNullOrEmpty(s))
                            .Select(s => char.ToUpper(RemoveDiacritics(s[0]))));
                        
                        // Nếu không tạo được tiền tố hợp lệ, sử dụng "SP"
                        if (string.IsNullOrEmpty(productPrefix))
                        {
                            productPrefix = "SP";
                        }
                        break;
                }

                int nextProductNumber = 1;

                try
                {
                    // Tìm ProductCode lớn nhất cho tiền tố này
                    var lastProductCode = await _context.Products
                        .Where(p => p.ProductCode.StartsWith(productPrefix))
                        .OrderByDescending(p => p.ProductCode)
                        .Select(p => p.ProductCode)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(lastProductCode))
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

                // Tạo mã sản phẩm mới với định dạng theo tiền tố danh mục
                string newProductCode = $"{productPrefix}{nextProductNumber:D3}";
                Console.WriteLine($"Mã sản phẩm mới: {newProductCode}");

                // Tạo ngày nhập kho với độ chính xác cao (bao gồm mili giây)
                DateTime importDate = DateTime.Now;

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
                    
                    SkinType = productDto.SkinType,
                    Description = productDto.Description,
                    Ingredients = productDto.Ingredients,
                    UsageInstructions = productDto.UsageInstructions,
                    ManufactureDate = productDto.ManufactureDate,
                    ImportDate = importDate // Sử dụng ngày hiện tại với độ chính xác cao
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

        // Thêm hàm này để loại bỏ dấu
        private static char RemoveDiacritics(char c)
        {
            string cStr = c.ToString();
            string normalized = cStr.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (char ch in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC)[0];
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

        //[HttpPut("{id}/product")]
        //public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpdateDTO productDto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var product = await _context.Products.FindAsync(id);
        //    if (product == null)
        //    {
        //        return NotFound(new { message = "Sản phẩm không tồn tại." });
        //    }

        //    try
        //    {
        //        // Lưu lại giá trị cũ của Quantity
        //        int oldQuantity = product.Quantity;

        //        // Cập nhật dữ liệu từ DTO
        //        product.ProductName = productDto.ProductName;
        //        product.CategoryId = productDto.CategoryId;
        //        product.Quantity = productDto.Quantity;
        //        product.Capacity = productDto.Capacity;
        //        product.Price = productDto.Price;
        //        product.Brand = productDto.Brand;
        //        product.Origin = productDto.Origin;
        //        product.Status = productDto.Status;

        //        product.SkinType = productDto.SkinType;
        //        product.Description = productDto.Description;
        //        product.Ingredients = productDto.Ingredients;
        //        product.UsageInstructions = productDto.UsageInstructions;
        //        product.ManufactureDate = productDto.ManufactureDate;

        //        // Cập nhật ImportDate nếu số lượng tăng lên
        //        if (productDto.Quantity > oldQuantity)
        //        {
        //            product.ImportDate = DateTime.Now; // Cập nhật thời gian nhập kho với độ chính xác cao
        //        }

        //        await _context.SaveChangesAsync();

        //        return Ok(new { message = "Cập nhật sản phẩm thành công." });
        //    }
        //    catch (DbUpdateException ex)
        //    {
        //        return StatusCode(500, new { error = "Lỗi khi lưu dữ liệu vào database.", details = ex.InnerException?.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { error = "Lỗi không xác định.", details = ex.Message });
        //    }
        //}

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
                int oldQuantity = product.Quantity;

                // Nếu CategoryId thay đổi, cập nhật ProductCode mới
                if (product.CategoryId != productDto.CategoryId)
                {
                    var category = await _context.Categories.FindAsync(productDto.CategoryId);
                    if (category == null)
                    {
                        return BadRequest(new { error = "Danh mục không tồn tại" });
                    }

                    // Tạo tiền tố mới
                    var categoryPrefixes = new Dictionary<string, string>
            {
                { "Làm Sạch Da", "LSD" },
                { "Đặc Trị", "ĐT" },
                { "Dưỡng Ẩm", "DA" },
                { "Bộ Chăm Sóc Da Mặt", "BCSDM" },
                { "Chống Nắng Da Mặt", "CNDM" },
                { "Dưỡng Mắt", "DM" },
                { "Dụng Cụ/Phụ Kiện Chăm Sóc Da", "PKCSD" },
                { "Vấn Đề Về Da", "VDVD" },
                { "Dưỡng Môi", "DMI" },
                { "Mặt Nạ", "MN" }
            };

                    string productPrefix = categoryPrefixes.TryGetValue(category.CategoryType, out string prefix)
                        ? prefix
                        : string.Concat(category.CategoryType.Split(' ').Select(s => char.ToUpper(RemoveDiacritics(s[0]))));
                    productPrefix = string.IsNullOrEmpty(productPrefix) ? "SP" : productPrefix;

                    // Tìm mã sản phẩm lớn nhất trong danh mục mới
                    var lastProductCode = await _context.Products
                        .Where(p => p.ProductCode.StartsWith(productPrefix))
                        .OrderByDescending(p => p.ProductCode)
                        .Select(p => p.ProductCode)
                        .FirstOrDefaultAsync();

                    int nextProductNumber = 1;
                    if (!string.IsNullOrEmpty(lastProductCode))
                    {
                        string numericPart = lastProductCode.Substring(productPrefix.Length);
                        if (int.TryParse(numericPart, out int lastNumber))
                        {
                            nextProductNumber = lastNumber + 1;
                        }
                    }

                    // Cập nhật ProductCode mới
                    product.ProductCode = $"{productPrefix}{nextProductNumber:D3}";
                }

                // Cập nhật dữ liệu khác từ DTO
                product.ProductName = productDto.ProductName;
                product.CategoryId = productDto.CategoryId;
                product.Quantity = productDto.Quantity;
                product.Capacity = productDto.Capacity;
                product.Price = productDto.Price;
                product.Brand = productDto.Brand;
                product.Origin = productDto.Origin;
                product.Status = productDto.Status;
                
                product.SkinType = productDto.SkinType;
                product.Description = productDto.Description;
                product.Ingredients = productDto.Ingredients;
                product.UsageInstructions = productDto.UsageInstructions;
                product.ManufactureDate = productDto.ManufactureDate;

                if (productDto.Quantity > oldQuantity)
                {
                    product.ImportDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật sản phẩm thành công.", newProductCode = product.ProductCode });
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

        [HttpGet("products/best-seller")]
        public async Task<IActionResult> GetBestSellingProducts()
        {
            var bestSellers = await _context.OrderItems
                .GroupBy(oi => oi.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(g => g.TotalQuantity)
                .Take(10) // Lấy 10 sản phẩm bán chạy nhất
                .ToListAsync();

            if (!bestSellers.Any())
                return NotFound("No products found.");

            return Ok(bestSellers);
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
        [HttpGet("revenue/monthly")]
        public async Task<IActionResult> GetMonthlyRevenue(int year, int month)
        {
            var revenue = await _context.Orders
                .Where(o => o.OrderDate.Year == year && o.OrderDate.Month == month && o.OrderStatus == "Completed")
                .SumAsync(o => o.TotalAmount);
            return Ok(revenue);
        }

        [HttpGet("orders/canceled")]
        public async Task<IActionResult> GetTotalCanceledOrders()
        {
            var canceledOrders = await _context.Orders
                .CountAsync(o => o.OrderStatus == "Canceled");
            return Ok(canceledOrders);
        }

        [HttpGet("orders/confirmed")]
        public async Task<IActionResult> GetTotalConfirmedOrders()
        {
            var confirmedOrders = await _context.Orders
                .CountAsync(o => o.OrderStatus == "Completed");
            return Ok(confirmedOrders);
        }

        [HttpGet("revenue/total")]
        public async Task<IActionResult> GetTotalRevenue()
        {
            var totalRevenue = await _context.Orders
                .Where(o => o.OrderStatus == "Completed")
                .SumAsync(o => o.TotalAmount);
            return Ok(totalRevenue);
        }

        [HttpGet("summary")]
        public async Task<ActionResult<IEnumerable<object>>> GetPaymentSummary()
        {
            var payments = await _context.Payments
                .Select(p => new
                {
                    p.PaymentDate,
                    p.Amount
                })
                .ToListAsync();

            return Ok(payments);
        }
    }
}



