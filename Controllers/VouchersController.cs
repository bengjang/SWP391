using lamlai.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace test2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoucherController : ControllerBase
    {
        private readonly TestContext _context;

        public VoucherController(TestContext context)
        {
            _context = context;
        }

        // GET: api/Voucher
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Voucher>>> GetVouchers()
        {
            return await _context.Vouchers.ToListAsync();
        }

        // GET: api/Voucher/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Voucher>> GetVoucher(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);

            if (voucher == null)
            {
                return NotFound();
            }

            return voucher;
        }
        // POST: api/Voucher
        [HttpPost]
        public async Task<ActionResult<VoucherDTO>> CreateVoucher([FromBody] VoucherDTO voucherDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var voucher = new Voucher
            {
                VoucherName = voucherDto.VoucherName,
                DiscountPercent = voucherDto.DiscountPercent,
                MinOrderAmount = voucherDto.MinOrderAmount,
                StartDate = voucherDto.StartDate,
                EndDate = voucherDto.EndDate,
                Quantity = voucherDto.Quantity,
                Status = voucherDto.Status,
                Description = voucherDto.Description
            };

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            // Chỉ trả về thông tin cần thiết
            return CreatedAtAction(nameof(GetVoucher), new { id = voucher.VoucherId }, new VoucherDTO
            {
                VoucherId = voucher.VoucherId,
                VoucherName = voucher.VoucherName,
                DiscountPercent = voucher.DiscountPercent,
                MinOrderAmount = voucher.MinOrderAmount,
                StartDate = voucher.StartDate,
                EndDate = voucher.EndDate,
                Quantity = voucher.Quantity,
                Status = voucher.Status,
                Description = voucher.Description
            });
        }


        // PUT: api/Voucher/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVoucher(int id, [FromBody] VoucherDTO voucherDto)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
            {
                return NotFound(new { message = "Voucher không tồn tại." });
            }

            voucher.VoucherName = voucherDto.VoucherName;
            voucher.DiscountPercent = voucherDto.DiscountPercent;
            voucher.MinOrderAmount = voucherDto.MinOrderAmount;
            voucher.StartDate = voucherDto.StartDate;
            voucher.EndDate = voucherDto.EndDate;
            voucher.Quantity = voucherDto.Quantity;
            voucher.Status = voucherDto.Status;
            voucher.Description = voucherDto.Description;

            await _context.SaveChangesAsync();

            // Trả về JSON gọn hơn
            return Ok(new VoucherDTO
            {
                VoucherId = voucher.VoucherId,
                VoucherName = voucher.VoucherName,
                DiscountPercent = voucher.DiscountPercent,
                MinOrderAmount = voucher.MinOrderAmount,
                StartDate = voucher.StartDate,
                EndDate = voucher.EndDate,
                Quantity = voucher.Quantity,
                Status = voucher.Status,
                Description = voucher.Description
            });
        }
        public class VoucherDTO
        {
            public int VoucherId { get; set; }
            public string VoucherName { get; set; } = null!;
            public decimal DiscountPercent { get; set; }
            public decimal? MinOrderAmount { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int? Quantity { get; set; }
            public string? Status { get; set; }
            public string? Description { get; set; }
        }



        // DELETE: api/Voucher/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVoucher(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }

            _context.Vouchers.Remove(voucher);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VoucherExists(int id)
        {
            return _context.Vouchers.Any(e => e.VoucherId == id);
        }
    }
}