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
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        //{
        //    return await _context.Orders.ToListAsync();
        //}

        //// GET: api/Orders/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<Order>> GetOrder(int id)
        //{
        //    var order = await _context.Orders.FindAsync(id);

        //    if (order == null)
        //    {
        //        return NotFound();
        //    }

        //    return order;
        //}
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Where(o => o.OrderId == orderId)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product) // Load thông tin sản phẩm
                    .FirstOrDefaultAsync();

                if (order == null)
                {
                    return NotFound(new { message = "Đơn hàng không tồn tại." });
                }

                var orderDto = new
                {
                    order.OrderId,
                    order.UserId,
                    order.OrderStatus,
                    order.TotalAmount,
                    Items = order.OrderItems.Select(oi => new
                    {
                        oi.OrderItemId,
                        oi.ProductId,
                        ProductName = oi.Product != null ? oi.Product.ProductName : "Sản phẩm không tồn tại",
                        oi.Quantity,
                        oi.Price
                    }).ToList()
                };

                return Ok(orderDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
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
                var product = await _context.Products.FindAsync(request.ProductId);
                if (product == null)
                    return NotFound("Không tìm thấy sản phẩm.");

                if (product.Quantity < request.Quantity)
                    return BadRequest("Số lượng sản phẩm không đủ.");

                // 🔍 Kiểm tra xem user đã có đơn hàng "Pending" chưa
                var order = await _context.Orders
                    .Where(o => o.UserId == request.UserId && o.OrderStatus == "Pending")
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync();

                if (order == null)
                {
                    // Nếu chưa có, tạo đơn hàng mới
                    order = new Order
                    {
                        UserId = request.UserId,
                        OrderDate = DateTime.UtcNow,
                        OrderStatus = "Pending",
                        TotalAmount = 0
                    };
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();
                }

                // 🔍 Kiểm tra sản phẩm đã có trong giỏ hàng chưa
                var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == request.ProductId);
                if (orderItem != null)
                {
                    // Nếu có, cập nhật số lượng
                    orderItem.Quantity += request.Quantity;
                    orderItem.Price = orderItem.Quantity * product.Price;
                }
                else
                {
                    // Nếu chưa có, thêm mới vào giỏ hàng
                    orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = request.ProductId,
                        ProductName = product.ProductName,
                        Quantity = request.Quantity,
                        Price = product.Price * request.Quantity
                    };
                    _context.OrderItems.Add(orderItem);
                }

                // ✅ Cập nhật tổng tiền đơn hàng
                order.TotalAmount = order.OrderItems.Sum(oi => (decimal?)oi.Price) ?? 0;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Sản phẩm đã được thêm vào giỏ hàng.", order });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpPut("updatecartitem")]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemRequest request)
        {
            try
            {
                var orderItem = await _context.OrderItems.FindAsync(request.OrderItemId);
                if (orderItem == null)
                    return NotFound("Không tìm thấy sản phẩm trong giỏ hàng.");

                var product = await _context.Products.FindAsync(orderItem.ProductId);
                if (product == null)
                    return NotFound("Sản phẩm không tồn tại.");

                var order = await _context.Orders.Include(o => o.OrderItems)
                                                 .FirstOrDefaultAsync(o => o.OrderId == orderItem.OrderId);
                if (order == null)
                    return NotFound("Không tìm thấy đơn hàng.");

                // 🚨 Kiểm tra trạng thái đơn hàng: Nếu đã thanh toán hoặc hoàn thành, không cho phép cập nhật
                if (order.OrderStatus == "Paid" || order.OrderStatus == "Completed")
                    return BadRequest("Không thể cập nhật giỏ hàng vì đơn hàng đã được thanh toán.");

                if (request.Quantity < 0)
                    return BadRequest("Số lượng sản phẩm phải lớn hơn 0.");

                if (request.Quantity > product.Quantity)
                    return BadRequest("Số lượng sản phẩm không đủ.");

                if (request.Quantity == 0)
                {
                    // 🗑 Nếu số lượng = 0, xóa sản phẩm khỏi giỏ hàng
                    _context.OrderItems.Remove(orderItem);
                }
                else
                {
                    // ✏ Nếu số lượng > 0, cập nhật lại giỏ hàng
                    orderItem.Quantity = request.Quantity;
                    orderItem.Price = product.Price * request.Quantity;
                }

                // ✅ Cập nhật tổng tiền đơn hàng
                order.TotalAmount = order.OrderItems.Sum(oi => (decimal?)oi.Price) ?? 0;

                // ✅ Lưu thay đổi vào database
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật giỏ hàng thành công.", order });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.InnerException?.Message ?? ex.Message}");
            }
        }


        [HttpDelete("removefromcart/{orderItemId}")]
        public async Task<IActionResult> RemoveFromCart(int orderItemId)
        {
            try
            {
                // 🔍 Tìm OrderItem cần xóa
                var orderItem = await _context.OrderItems.FindAsync(orderItemId);
                if (orderItem == null)
                    return NotFound("Không tìm thấy sản phẩm trong giỏ hàng.");

                // 🔍 Lấy Order tương ứng
                var order = await _context.Orders.FindAsync(orderItem.OrderId);
                if (order == null)
                    return NotFound("Không tìm thấy đơn hàng.");

                // 🗑 Xóa sản phẩm khỏi giỏ hàng
                _context.OrderItems.Remove(orderItem);
                await _context.SaveChangesAsync(); // ✅ Lưu ngay để cập nhật database

                // ✅ Cập nhật lại tổng tiền của Order sau khi xóa
                order.TotalAmount = await _context.OrderItems
                    .Where(oi => oi.OrderId == order.OrderId)
                    .SumAsync(oi => (decimal?)oi.Price) ?? 0;

                // 🗑 Nếu đơn hàng không còn sản phẩm nào → Xóa đơn hàng luôn
                if (!await _context.OrderItems.AnyAsync(oi => oi.OrderId == order.OrderId))
                {
                    _context.Orders.Remove(order);
                }

                await _context.SaveChangesAsync(); // ✅ Lưu thay đổi sau khi cập nhật Order

                return Ok(new { message = "Đã xóa sản phẩm khỏi giỏ hàng.", orderId = order.OrderId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.InnerException?.Message ?? ex.Message}");
            }
        }


        [HttpDelete("removeorderid/{orderId}")]
        public async Task<IActionResult> RemoveOrder(int orderId)
        {
            try
            {
                // 🔍 Tìm Order cần xóa
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                    return NotFound("Không tìm thấy đơn hàng.");

                // 🗑 Xóa tất cả OrderItems liên quan
                _context.OrderItems.RemoveRange(order.OrderItems);

                // 🗑 Xóa Order
                _context.Orders.Remove(order);

                await _context.SaveChangesAsync();
                return Ok(new { message = "Đơn hàng đã được xóa thành công.", orderId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpPost("applyvoucher")]
        public async Task<IActionResult> ApplyVoucher([FromBody] ApplyVoucherRequest request)
        {
            try
            {
                // 🔍 Kiểm tra đơn hàng có tồn tại không
                var order = await _context.Orders.FindAsync(request.OrderId);
                if (order == null)
                    return NotFound("Không tìm thấy đơn hàng.");

                // ❌ Kiểm tra trạng thái đơn hàng (chỉ áp dụng cho đơn hàng "Pending")
                if (order.OrderStatus != "Pending")
                    return BadRequest("Không thể áp dụng voucher cho đơn hàng đã thanh toán hoặc hoàn thành.");

                // 🔍 Kiểm tra voucher có hợp lệ không
                var voucher = await _context.Vouchers.FindAsync(request.VoucherId);
                if (voucher == null)
                    return NotFound("Voucher không tồn tại.");

                // 🕒 Kiểm tra ngày hợp lệ
                var now = DateTime.UtcNow;
                if (now < voucher.StartDate || now > voucher.EndDate)
                    return BadRequest("Voucher đã hết hạn hoặc chưa có hiệu lực.");

                // 💰 Kiểm tra điều kiện giá trị đơn hàng tối thiểu
                if (voucher.MinOrderAmount.HasValue && order.TotalAmount < voucher.MinOrderAmount.Value)
                    return BadRequest($"Đơn hàng chưa đạt giá trị tối thiểu {voucher.MinOrderAmount.Value:C} để áp dụng voucher.");

                // 🏷 Kiểm tra số lượng voucher còn lại (nếu có giới hạn)
                if (voucher.Quantity.HasValue && voucher.Quantity.Value <= 0)
                    return BadRequest("Voucher đã hết số lượng sử dụng.");

                // 💵 Tính giảm giá
                decimal discountAmount = (order.TotalAmount * voucher.DiscountPercent) / 100;
                decimal newTotalAmount = order.TotalAmount - discountAmount;

                // ✅ Cập nhật đơn hàng với voucher
                order.VoucherId = voucher.VoucherId;
                order.TotalAmount = newTotalAmount;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Áp dụng voucher thành công!", order });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
        // DTO nhận dữ liệu từ client
        public class ApplyVoucherRequest
        {
            public int OrderId { get; set; }
            public int VoucherId { get; set; }
        }

        [HttpGet("current/{userId}")]
        public async Task<IActionResult> GetCurrentCart(int userId)
        {
            try
            {
                var order = await _context.Orders
                    .Where(o => o.UserId == userId && o.OrderStatus == "Pending")
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product) // Load thông tin sản phẩm
                    .FirstOrDefaultAsync();

                if (order == null)
                {
                    return NotFound(new { message = "Giỏ hàng hiện tại trống." });
                }

                var cartDto = new
                {
                    order.OrderId,
                    order.UserId,
                    order.OrderStatus,
                    order.TotalAmount,
                    Items = order.OrderItems.Select(oi => new
                    {
                        oi.OrderItemId,
                        oi.ProductId,
                        oi.Product.ProductName,
                        oi.Quantity,
                        oi.Price
                    }).ToList()
                };

                return Ok(cartDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> a()
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product) // Load thông tin sản phẩm
                    .ToListAsync();

                var orderDtos = orders.Select(o => new
                {
                    o.OrderId,
                    o.UserId,
                    o.OrderStatus,
                    o.TotalAmount,
                    Items = o.OrderItems.Select(oi => new
                    {
                        oi.OrderItemId,
                        oi.ProductId,
                        ProductName = oi.Product != null ? oi.Product.ProductName : "Sản phẩm không tồn tại",
                        oi.Quantity,
                        oi.Price
                    }).ToList()
                });

                return Ok(orderDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }


        [HttpPost("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.Voucher) // ✅ Load Voucher nếu có
                    .FirstOrDefaultAsync(o => o.OrderId == request.OrderId && o.OrderStatus == "Pending");

                if (order == null)
                    return NotFound("Không tìm thấy đơn hàng hoặc đơn hàng không hợp lệ.");

                // ✅ Kiểm tra từng sản phẩm trong đơn hàng trước khi trừ kho
                foreach (var orderItem in order.OrderItems)
                {
                    var product = await _context.Products.FindAsync(orderItem.ProductId);
                    if (product == null)
                        return NotFound($"Sản phẩm {orderItem.ProductName} không tồn tại.");

                    if (product.Quantity < orderItem.Quantity)
                        return BadRequest($"Sản phẩm {orderItem.ProductName} không đủ số lượng trong kho.");

                    // 🔽 Trừ số lượng sản phẩm trong kho
                    product.Quantity -= orderItem.Quantity;
                }

                // ✅ Áp dụng voucher nếu có
                if (order.VoucherId.HasValue)
                {
                    var voucher = order.Voucher;

                    if (voucher == null)
                        return BadRequest("Voucher không hợp lệ.");

                    // Kiểm tra ngày hết hạn
                    var now = DateTime.UtcNow;
                    if (now < voucher.StartDate || now > voucher.EndDate)
                        return BadRequest("Voucher đã hết hạn hoặc chưa có hiệu lực.");

                    // Kiểm tra điều kiện giá trị đơn hàng tối thiểu
                    if (voucher.MinOrderAmount.HasValue && order.TotalAmount < voucher.MinOrderAmount.Value)
                        return BadRequest($"Đơn hàng chưa đạt giá trị tối thiểu {voucher.MinOrderAmount.Value:C} để áp dụng voucher.");

                    // ✅ Tính tổng tiền sau khi giảm giá
                    var discountAmount = (order.TotalAmount * voucher.DiscountPercent) / 100;
                    order.TotalAmount -= discountAmount;

                    // Nếu voucher có giới hạn số lượng, giảm số lượng voucher
                    if (voucher.Quantity.HasValue && voucher.Quantity > 0)
                        voucher.Quantity -= 1;
                }

                // ✅ Cập nhật trạng thái đơn hàng
                order.OrderStatus = "Paid"; // Đánh dấu đơn hàng đã thanh toán
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thanh toán thành công!", order });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.InnerException?.Message ?? ex.Message}");
            }
        }


        // ✅ Định nghĩa class ngay trong API
        public class ConfirmPaymentRequest
        {
            public int OrderId { get; set; }
        }


    }

    // DTO nhận dữ liệu từ client
    public class UpdateCartItemRequest
    {
        public int OrderItemId { get; set; }
        public int Quantity { get; set; }
    }


}

