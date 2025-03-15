﻿using System;
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
        // Thêm sản phẩm mới
        [HttpPost("Product")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductUpdateDTO productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Gọi stored procedure để lấy ProductCode
                var productCodeParam = new SqlParameter("@NewProductCode", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };

                await _context.Database.ExecuteSqlRawAsync(
     "EXEC AddProduct @CategoryID, @ProductName, @Quantity, @Capacity, @Price, @Brand, @Origin, @Status, @ImgURL",
     new SqlParameter("@CategoryID", productDto.CategoryId),
     new SqlParameter("@ProductName", productDto.ProductName),
     new SqlParameter("@Quantity", productDto.Quantity),
     new SqlParameter("@Capacity", productDto.Capacity),
     new SqlParameter("@Price", productDto.Price),
     new SqlParameter("@Brand", productDto.Brand),
     new SqlParameter("@Origin", productDto.Origin),
     new SqlParameter("@Status", productDto.Status),
     new SqlParameter("@ImgURL", productDto.ImgUrl)
 );


                // Lấy giá trị ProductCode từ stored procedure
                var newProductCode = (string)productCodeParam.Value;

                // Tạo sản phẩm và gán ProductCode
                var product = new Product
                {
                    ProductCode = newProductCode, // Gán ProductCode từ stored procedure
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
                    ManufactureDate = productDto.ManufactureDate
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
            product.Status = (product.Status == "Available") ? "Unavailable" : "Available";

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
       


    }
}



