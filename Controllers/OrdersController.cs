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

namespace test2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly TestContext _context;

        public OrdersController(TestContext context)
        {
            _context = context;
        }

        //[HttpPost("addtocart")]
        //[Authorize]
        //public async Task<IActionResult> AddToCart(int productId, int quantity)
        //{
        //    try
        //    {
        //        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        //        {
        //            return BadRequest("Người dùng không hợp lệ.");
        //        }

        //        var product = await _context.Products.FindAsync(productId);
        //        if (product == null)
        //        {
        //            return NotFound("Không tìm thấy sản phẩm.");
        //        }

        //        if (product.Quantity < quantity)
        //        {
        //            return BadRequest("Số lượng sản phẩm không đủ.");
        //        }

        //        // Create the cart item information to return
        //        var cartItem = new
        //        {
        //            ProductId = product.ProductId,
        //            ProductName = product.ProductName,
        //            Quantity = quantity,
        //            Price = product.Price
        //        };

        //        return Ok(new { message = "Sản phẩm đã được thêm vào giỏ hàng.", cartItem });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Đã xảy ra lỗi khi thêm sản phẩm vào giỏ hàng: {ex.Message}");
        //    }
        //}

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders.ToListAsync();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // PUT: api/Orders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            if (id != order.OrderId)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
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

        // POST: api/Orders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrder", new { id = order.OrderId }, order);
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
        public class AddToCartRequest
        {
            public int UserId { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }

        [HttpPost("addtocart")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                // Kiểm tra sản phẩm
                var product = await _context.Products.FindAsync(request.ProductId);
                if (product == null)
                    return NotFound("Không tìm thấy sản phẩm.");

                if (product.Quantity < request.Quantity)
                    return BadRequest("Số lượng sản phẩm không đủ.");

                // Tạo đơn hàng
                var order = new Order
                {
                    UserId = request.UserId,
                    OrderDate = DateTime.Now,
                    OrderStatus = "Pending",
                    TotalAmount = product.Price * request.Quantity,
                    OrderItems = new List<OrderItem>() // Khởi tạo danh sách OrderItems
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ Order saved successfully with ID: {order.OrderId}");

                // Tạo OrderItem
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = request.ProductId,
                    ProductName = product.ProductName,
                    Quantity = request.Quantity,
                    Price = product.Price
                };

                _context.OrderItems.Add(orderItem);
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ OrderItem saved successfully for OrderId: {order.OrderId}");

                return Ok(new { message = "Sản phẩm đã được thêm vào giỏ hàng.", order });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.InnerException?.Message ?? ex.Message}");
                return StatusCode(500, $"Lỗi: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpPut("updatecartitem")]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemRequest request)
        {
            try
            {
                // Kiểm tra OrderItem có tồn tại không
                var orderItem = await _context.OrderItems.FindAsync(request.OrderItemId);
                if (orderItem == null)
                    return NotFound("Không tìm thấy sản phẩm trong giỏ hàng.");

                // Kiểm tra sản phẩm có tồn tại không
                var product = await _context.Products.FindAsync(orderItem.ProductId);
                if (product == null)
                    return NotFound("Sản phẩm không tồn tại.");

                // Kiểm tra số lượng hợp lệ
                if (request.Quantity <= 0)
                    return BadRequest("Số lượng sản phẩm phải lớn hơn 0.");

                if (request.Quantity > product.Quantity)
                    return BadRequest("Số lượng sản phẩm không đủ trong kho.");

                // Cập nhật số lượng và giá
                orderItem.Quantity = request.Quantity;
                orderItem.Price = product.Price * request.Quantity;

                // Cập nhật lại tổng tiền của đơn hàng
                var order = await _context.Orders.FindAsync(orderItem.OrderId);
                if (order != null)
                {
                    order.TotalAmount = _context.OrderItems
                        .Where(oi => oi.OrderId == order.OrderId)
                        .Sum(oi => (decimal?)oi.Price) ?? 0; // Nếu tất cả đều null thì trả về 0
                }
                else
                {
                    return NotFound("Không tìm thấy đơn hàng.");
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Cập nhật giỏ hàng thành công.", orderItem });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi: {ex.InnerException?.Message ?? ex.Message}");
                return StatusCode(500, $"Lỗi: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        // DTO nhận dữ liệu từ client
        public class UpdateCartItemRequest
        {
            public int OrderItemId { get; set; }
            public int Quantity { get; set; }
        }


    }
}
