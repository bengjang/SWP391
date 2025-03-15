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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CancelRequestDto>>> GetCancelRequests()
        {
            var cancelRequests = await _context.CancelRequests
                .Select(cr => new CancelRequestDto
                {
                    CancelRequestId = cr.CancelRequestId,
                    OrderId = cr.OrderId,
                    FullName = cr.FullName,
                    Phone = cr.Phone,
                    Reason = cr.Reason,
                    RequestDate = cr.RequestDate,
                    Status = cr.Status
                })
                .ToListAsync();

            return Ok(cancelRequests);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<CancelRequestDto>> GetCancelRequest(int id)
        {
            var cancelRequest = await _context.CancelRequests
                .Where(cr => cr.CancelRequestId == id)
                .Select(cr => new CancelRequestDto
                {
                    CancelRequestId = cr.CancelRequestId,
                    OrderId = cr.OrderId,
                    FullName = cr.FullName,
                    Phone = cr.Phone,
                    Reason = cr.Reason,
                    RequestDate = cr.RequestDate,
                    Status = cr.Status
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
        }

        // API gửi yêu cầu hủy đơn hàng (chỉ khi OrderStatus = "Paid")
        [HttpPost("request-cancel")]
        public async Task<IActionResult> RequestCancel([FromBody] CancelRequestDto request)
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

            // Tạo yêu cầu hủy đơn hàng
            var cancelRequest = new CancelRequest
            {
                OrderId = request.OrderId,
                FullName = request.FullName,
                Phone = request.Phone,
                Reason = request.Reason,
                RequestDate = DateTime.UtcNow,
                Status = "Pending"
            };

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
        public int OrderId { get; set; }
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Reason { get; set; } = null!;
    }
}
