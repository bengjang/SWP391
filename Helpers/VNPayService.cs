using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;

namespace WebAPI_FlowerShopSWP.Helpers
{
    public class VNPayService
    {
        public string TmnCode { get; }
        public string HashSecret { get; }
        public string Url { get; }
        public string ReturnUrl { get; }

        public VNPayService(IConfiguration configuration)
        {
            TmnCode = configuration["VNPay:TmnCode"];
            HashSecret = configuration["VNPay:HashSecret"];
            Url = configuration["VNPay:BaseUrl"];
            ReturnUrl = configuration["VNPay:PaymentBackReturnUrl"];
        }

        public string CreatePaymentUrl(decimal amount, string orderId)
        {
            var vnpayData = new SortedList<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", TmnCode },
                { "vnp_Amount", ((int)(amount * 100)).ToString() }, // Convert to smallest unit
                { "vnp_CurrCode", "VND" },
                { "vnp_TxnRef", orderId },
                { "vnp_OrderInfo", "Payment for order " + orderId },
                { "vnp_Locale", "vn" },
                { "vnp_ReturnUrl", ReturnUrl },
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") }
            };

            var query = new StringBuilder();
            foreach (var kv in vnpayData)
            {
                query.AppendFormat("{0}={1}&", kv.Key, kv.Value);
            }

            var signData = query.ToString().TrimEnd('&');
            var vnp_SecureHash = ComputeHash(signData);
            return $"{Url}?{signData}&vnp_SecureHash={vnp_SecureHash}";
        }

        private string ComputeHash(string data)
        {
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(HashSecret)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        // Add methods for VNPay processing here
    }
} 