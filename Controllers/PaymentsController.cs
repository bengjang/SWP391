using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using lamlai.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebAPI_FlowerShopSWP.Helpers;
using WebAPI_FlowerShopSWP.Configurations;
using Azure.Core;

namespace test2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly TestContext _context;
        private readonly VNPayConfig _vnpayConfig;
        private readonly ILogger<PaymentsController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly WebAPI_FlowerShopSWP.Helpers.VNPayService _vnpayService;

        public PaymentsController(TestContext context,
            IOptions<VNPayConfig> vnpayConfig,
            ILogger<PaymentsController> logger,
            IMemoryCache memoryCache,
            WebAPI_FlowerShopSWP.Helpers.VNPayService vnpayService)
        {
            _context = context;
            _vnpayConfig = vnpayConfig.Value;
            _logger = logger;
            _memoryCache = memoryCache;
            _vnpayService = vnpayService;
        }

        // GET: api/Payments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPayments()
        {
            return await _context.Payments.ToListAsync();
        }

        // GET: api/Payments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);

            if (payment == null)
            {
                return NotFound();
            }

            return payment;
        }

        // PUT: api/Payments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPayment(int id, Payment payment)
        {
            if (id != payment.PaymentId)
            {
                return BadRequest();
            }

            _context.Entry(payment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentExists(id))
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

        // POST: api/Payments
        [HttpPost]
        public async Task<ActionResult<Payment>> PostPayment(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPayment", new { id = payment.PaymentId }, payment);
        }

        // DELETE: api/Payments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Payments/createVnpPayment
        [HttpPost("createVnpPayment")]
        public async Task<IActionResult> CreateVnpPayment([FromBody] PaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.Amount <= 0)
            {
                return BadRequest("Amount must be greater than 0.");
            }

            var vnpay = new VnPayLibrary();
            // var vnp_Returnurl = "https://localhost:7175/api/Payments/vnpay-return"; // URL trả về sau khi thanh toán
            var vnp_Returnurl = request.ReturnUrl;
            var vnp_TxnRef = DateTime.Now.Ticks.ToString(); // Mã giao dịch
            var vnp_OrderInfo = request.OrderId.ToString();

            var vnp_OrderType = "other"; // Loại đơn hàng
            var vnp_Amount = request.Amount * 100; // Số tiền
            var vnp_Locale = "vn"; // Ngôn ngữ
            var vnp_IpAddr = HttpContext.Connection.RemoteIpAddress?.ToString(); // Địa chỉ IP

            string vnp_Url = _vnpayConfig.Url; // URL VNPay
            string vnp_HashSecret = _vnpayConfig.HashSecret; // Mật khẩu bảo mật

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", _vnpayConfig.TmnCode);
            vnpay.AddRequestData("vnp_Amount", vnp_Amount.ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", vnp_IpAddr);
            vnpay.AddRequestData("vnp_Locale", vnp_Locale);
            vnpay.AddRequestData("vnp_OrderInfo", vnp_OrderInfo);
            vnpay.AddRequestData("vnp_OrderType", vnp_OrderType);
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", vnp_TxnRef);

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return Ok(new { paymentUrl });
        }

        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VnPayReturn()
        {
            var vnpay = new VnPayLibrary();

            // Add response data từ query string của VNPay
            foreach (var (key, value) in Request.Query)
            {
                vnpay.AddResponseData(key, value.ToString());
            }

            var vnp_SecureHash = Request.Query["vnp_SecureHash"].ToString();
            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnp_OrderInfo = vnpay.GetResponseData("vnp_OrderInfo");
            var returnUrl = Request.Query["vnp_ReturnUrl"].ToString(); // Lấy ReturnUrl từ query string

            var checkSignature = vnpay.ValidateSignature(vnp_SecureHash, _vnpayConfig.HashSecret);
            if (!checkSignature)
            {
                _logger.LogWarning("Invalid VNPay signature");
                return BadRequest(new { status = "error", message = "Invalid signature" });
            }

            // Lấy OrderId từ vnp_OrderInfo
            var orderIdStr = vnp_OrderInfo.Replace("Thanh toán cho đơn hàng ", "").Trim();
            if (!int.TryParse(orderIdStr, out int orderId))
            {
                _logger.LogError("Cannot extract OrderId from OrderInfo: {OrderInfo}", vnp_OrderInfo);
                return BadRequest(new { status = "error", message = "Invalid OrderId in OrderInfo." });
            }

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                _logger.LogError("Order not found for OrderId: {OrderId}", orderId);
                return BadRequest(new { status = "error", message = "Order not found." });
            }

            if (vnp_ResponseCode == "00") // ✅ Thanh toán thành công
            {
                try
                {
                    var payment = new Payment
                    {
                        OrderId = order.OrderId,
                        Amount = decimal.Parse(vnpay.GetResponseData("vnp_Amount")) / 100, // Lấy amount từ response data
                        PaymentDate = DateTime.Now,
                        PaymentStatus = "Success"
                    };

                    order.OrderStatus = "Completed";
                    _context.Payments.Add(payment);
                    await _context.SaveChangesAsync();

                    // Sử dụng returnUrl từ query string
                    return Redirect($"{returnUrl}?status=success&message=Thanh toán thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing payment");
                    return StatusCode(500, new { status = "error", message = "Error processing payment." });
                }
            }
            else
            {
                var failedPayment = new Payment
                {
                    OrderId = order.OrderId,
                    //Amount = decimal.Parse(vnp_Amount) / 100,
                    Amount = decimal.Parse(vnpay.GetResponseData("vnp_Amount")) / 100,
                    PaymentDate = DateTime.Now,
                    PaymentStatus = "Failed"
                };

                try
                {
                    _context.Payments.Add(failedPayment);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error recording failed payment");
                }

                // Sử dụng returnUrl từ query string
                return Redirect($"{returnUrl}?status=error&message=Thanh toán không thành công");
            }
        }

        [HttpPost("confirmCodPayment")]
        public async Task<IActionResult> ConfirmCodPayment([FromBody] CodPaymentRequest request)
        {
            try
            {
                // Log thông tin để debug
                _logger.LogInformation($"Xử lý thanh toán COD cho đơn hàng {request.OrderId} với số tiền {request.Amount}");

                // Kiểm tra order có tồn tại không
                var order = await _context.Orders.FindAsync(request.OrderId);
                if (order == null)
                {
                    _logger.LogWarning($"Không tìm thấy đơn hàng với ID {request.OrderId}");
                    return NotFound(new { message = "Không tìm thấy đơn hàng." });
                }

                // Kiểm tra xem đơn hàng đã có payment chưa
                var existingPayment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.OrderId == request.OrderId);

                if (existingPayment != null)
                {
                    _logger.LogInformation($"Đơn hàng {request.OrderId} đã có bản ghi payment, trả về payment hiện có");
                    return Ok(new { success = true, message = "Đơn hàng đã có bản ghi thanh toán.", payment = existingPayment });
                }

                // Tạo bản ghi payment mới
                var payment = new Payment
                {
                    OrderId = request.OrderId,
                    Amount = request.Amount,
                    PaymentDate = DateTime.UtcNow,
                    PaymentStatus = "Success"
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Đã tạo bản ghi payment thành công cho đơn hàng {request.OrderId}");

                return Ok(new { success = true, message = "Đã ghi nhận thanh toán COD.", payment });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xử lý thanh toán COD cho đơn hàng {request.OrderId}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        public class PaymentRequest
        {
            public decimal Amount { get; set; }
            public string OrderId { get; set; }
            public string ReturnUrl { get; set; }//new
        }

        public class CodPaymentRequest
        {
            public int OrderId { get; set; }
            public decimal Amount { get; set; }
        }

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.PaymentId == id);
        }
    }
}
