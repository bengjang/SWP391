﻿using lamlai.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public async Task<ActionResult<VoucherDTO>> CreateVoucher([FromBody] VoucherUpdateDTO voucherDto)
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


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVoucher(int id, [FromBody] VoucherUpdateDTO voucherDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
            {
                return NotFound(new { error = "Voucher không tồn tại." });
            }

            try
            {
                // Chỉ cập nhật các trường từ DTO, KHÔNG thay đổi voucherId
                voucher.VoucherName = voucherDto.VoucherName;
                voucher.DiscountPercent = voucherDto.DiscountPercent;
                voucher.MinOrderAmount = voucherDto.MinOrderAmount;
                voucher.StartDate = voucherDto.StartDate;
                voucher.EndDate = voucherDto.EndDate;
                voucher.Quantity = voucherDto.Quantity;
                voucher.Status = voucherDto.Status;
                voucher.Description = voucherDto.Description;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật voucher thành công." });
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
        public class VoucherUpdateDTO
        {
            [Required]
            public string VoucherName { get; set; } = null!;

            [Range(0, 100, ErrorMessage = "Giá trị giảm giá phải từ 0 đến 100.")]
            public decimal DiscountPercent { get; set; }

            public decimal? MinOrderAmount { get; set; }

            [Required]
            public DateTime StartDate { get; set; }

            [Required]
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
        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleVoucherStatus(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
            {
                return NotFound(new { error = "Voucher không tồn tại." });
            }

            // Đảo trạng thái giữa "Active" và "Inactive"
            voucher.Status = (voucher.Status == "Active") ? "Inactive" : "Active";

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Trạng thái voucher đã được cập nhật.", newStatus = voucher.Status });
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