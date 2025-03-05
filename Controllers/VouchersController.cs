using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using lamlai2.Models;

namespace lamlai2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VouchersController : ControllerBase
    {
        private readonly TestContext _context;

        public VouchersController(TestContext context)
        {
            _context = context;
        }

        // ✅ Lấy danh sách tất cả voucher
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Voucher>>> GetVouchers()
        {
            return await _context.Vouchers.ToListAsync();
        }

        // ✅ Lấy thông tin voucher theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Voucher>> GetVoucher(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);

            if (voucher == null)
            {
                return NotFound("Voucher không tồn tại.");
            }

            return voucher;
        }

        // ✅ Thêm voucher mới
        [HttpPost]
        public async Task<ActionResult<Voucher>> PostVoucher(Voucher voucher)
        {
            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVoucher), new { id = voucher.VoucherId }, voucher);
        }

        // ✅ Cập nhật voucher
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVoucher(int id, Voucher voucher)
        {
            if (id != voucher.VoucherId)
            {
                return BadRequest("ID không khớp.");
            }

            _context.Entry(voucher).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VoucherExists(id))
                {
                    return NotFound("Voucher không tồn tại.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // ✅ Xóa voucher
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVoucher(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
            {
                return NotFound("Voucher không tồn tại.");
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
