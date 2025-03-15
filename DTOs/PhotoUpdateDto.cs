using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace SWP391.DTOs
{
    public class PhotoUpdateDto
    {
        [Required(ErrorMessage = "File ảnh không được để trống")]
        public IFormFile File { get; set; }

        [Range(0, 4, ErrorMessage = "Thứ tự hiển thị phải từ 0 đến 4")]
        public int DisplayOrder { get; set; }

        // Validation cho file ảnh
        public static bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // Kiểm tra kích thước file (tối đa 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return false;

            // Kiểm tra định dạng file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                return false;

            return true;
        }
    }
} 