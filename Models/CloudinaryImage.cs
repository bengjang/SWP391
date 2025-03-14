using System;

namespace SWP391.Models
{
    public class CloudinaryImage
    {
        public int Id { get; set; }
        public string PublicId { get; set; }
        public string Url { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Foreign key cho các entity khác nếu cần
        // Ví dụ: public int ProductId { get; set; }
        // public Product Product { get; set; }
    }
} 