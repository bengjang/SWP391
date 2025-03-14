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

    }
}

