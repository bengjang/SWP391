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
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDTO productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var product = new Product
            {
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

        // Cập nhật sản phẩm
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDTO productDto)
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
            return NoContent();
        }

        public class ProductDTO
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public int CategoryId { get; set; }
            public int Quantity { get; set; }
            public string Capacity { get; set; }
            public decimal Price { get; set; }
            public string Brand { get; set; }
            public string Origin { get; set; }
            public string Status { get; set; }
            public string ImgUrl { get; set; }
            public string SkinType { get; set; }
            public string Description { get; set; }
            public string Ingredients { get; set; }
            public string UsageInstructions { get; set; }
            public DateTime ManufactureDate { get; set; }
        }


    
     
        }
    }



