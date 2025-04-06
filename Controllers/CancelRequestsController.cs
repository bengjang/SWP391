using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using lamlai.Models;

namespace lamlai.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CancelRequestsController : ControllerBase
    {
        private readonly TestContext _context;

        public CancelRequestsController(TestContext context)
        {
            _context = context;
        }

        public class OrderItemDto
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = null!;
            public int Quantity { get; set; }
            public decimal Price { get; set; }
        }

        public class OrderDto
        {
            public int OrderId { get; set; }
            public string OrderStatus { get; set; } = null!;
            public DateTime OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string DeliveryStatus { get; set; } = null!;
            public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CancelRequestDto>>> GetCancelRequests()
        {
            var cancelRequests = await _context.CancelRequests
                .Include(cr => cr.Order)
                .Select(cr => new CancelRequestDto
                {
                    CancelRequestId = cr.CancelRequestId,
                    OrderId = cr.OrderId,
                    FullName = cr.FullName,
                    Phone = cr.Phone,
                    Reason = cr.Reason,
                    RequestDate = cr.RequestDate,
                    Status = cr.Status,
                    OrderDetails = new OrderDto
                    {
                        OrderId = cr.Order.OrderId,
                        OrderStatus = cr.Order.OrderStatus,
                        OrderDate = cr.Order.OrderDate,
                        TotalAmount = cr.Order.TotalAmount,
                        DeliveryStatus = cr.Order.DeliveryStatus,
                        OrderItems = cr.Order.OrderItems.Select(oi => new OrderItemDto
                        {
                            ProductId = oi.ProductId,
                            ProductName = oi.Product.ProductName,
                            Quantity = oi.Quantity,
                            Price = oi.Price
                        }).ToList()
                    }
                })
                .ToListAsync();

            return Ok(cancelRequests);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CancelRequestDto>> GetCancelRequest(int id)
        {
            var cancelRequest = await _context.CancelRequests
                .Include(cr => cr.Order)
                .Where(cr => cr.CancelRequestId == id)
                .Select(cr => new CancelRequestDto
                {
                    CancelRequestId = cr.CancelRequestId,
                    OrderId = cr.OrderId,
                    FullName = cr.FullName,
                    Phone = cr.Phone,
                    Reason = cr.Reason,
                    RequestDate = cr.RequestDate,
                    Status = cr.Status,
                    OrderDetails = new
                    {
                        cr.Order.OrderId,
                        cr.Order.OrderStatus,
                        cr.Order.TotalAmount,
                        cr.Order.OrderDate,
                    }
                })
                .FirstOrDefaultAsync();

            if (cancelRequest == null)
            {
                return NotFound(new { error = "Yêu cầu hủy không tồn tại." });
            }

            return Ok(cancelRequest);
        }

        public class CancelRequestDto
        {
            public int CancelRequestId { get; set; }
            public int OrderId { get; set; }
            public string FullName { get; set; } = null!;
            public string Phone { get; set; } = null!;
            public string Reason { get; set; } = null!;
            public DateTime RequestDate { get; set; }
            public string Status { get; set; } = null!;
            public object OrderDetails { get; set; }
        }
        public class CancelRequestDto2
        {
           
            public int OrderId { get; set; }
         
            public string Reason { get; set; } = null!;
            
            
        }
        [HttpPost("request-cancel")]
        public async Task<IActionResult> RequestCancel([FromBody] CancelRequestDto2 request)
        {
            // Kiểm tra đơn hàng có tồn tại không
            var order = await _context.Orders.FindAsync(request.OrderId);
            if (order == null)
            {
                return NotFound(new { error = "Đơn hàng không tồn tại." });
            }

            // Chỉ cho phép hủy nếu trạng thái đơn hàng là "Paid"
            if (order.OrderStatus != "Paid")
            {
                return BadRequest(new { error = "Chỉ có thể hủy đơn hàng có trạng thái 'Paid'." });
            }

            // Kiểm tra thời gian đặt hàng
            if (DateTime.UtcNow - order.OrderDate > TimeSpan.FromHours(24))
            {
                return BadRequest(new { error = "Không thể hủy đơn hàng sau 24 tiếng kể từ khi đặt." });
            }

            // Kiểm tra nếu đã có bất kỳ yêu cầu hủy nào cho đơn hàng này
            var existingCancelRequest = await _context.CancelRequests
                .AnyAsync(cr => cr.OrderId == request.OrderId);

            if (existingCancelRequest)
            {
                return BadRequest(new { error = "Bạn chỉ có thể gửi yêu cầu hủy một lần cho đơn hàng này." });
            }

            // Lấy thông tin người dùng từ UserId
            var user = await _context.Users.FindAsync(order.UserId);
            if (user == null)
            {
                return NotFound(new { error = "Người dùng không tồn tại." });
            }

            // Tạo yêu cầu hủy đơn hàng
            var cancelRequest = new CancelRequest
            {
                OrderId = request.OrderId,
                FullName = user.FullName, // Lấy FullName từ User
                Phone = user.Phone, // Lấy Phone từ User
                Reason = request.Reason,
                RequestDate = DateTime.UtcNow, // Set the current date and time
                Status = "Pending"
            };

            // Cập nhật trạng thái đơn hàng thành 'Cancelling'
            order.OrderStatus = "Cancelling";

            _context.CancelRequests.Add(cancelRequest);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Yêu cầu hủy đơn hàng đã được gửi thành công.", cancelRequestId = cancelRequest.CancelRequestId });
        }


        // PUT: api/CancelRequests/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCancelRequest(int id, CancelRequest cancelRequest)
        {
            if (id != cancelRequest.CancelRequestId)
            {
                return BadRequest();
            }

            _context.Entry(cancelRequest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CancelRequestExists(id))
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

        // DELETE: api/CancelRequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCancelRequest(int id)
        {
            var cancelRequest = await _context.CancelRequests.FindAsync(id);
            if (cancelRequest == null)
            {
                return NotFound();
            }

            _context.CancelRequests.Remove(cancelRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CancelRequestExists(int id)
        {
            return _context.CancelRequests.Any(e => e.CancelRequestId == id);
        }
    }

    // DTO để nhận dữ liệu từ request
    public class CancelRequestDto
    {
        public int CancelRequestId { get; set; }
        public int OrderId { get; set; }
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Reason { get; set; } = null!;
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = null!;
        public object OrderDetails { get; set; }
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class OrderDto
    {
        public int OrderId { get; set; }
        public string OrderStatus { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string DeliveryStatus { get; set; } = null!;
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    }
}