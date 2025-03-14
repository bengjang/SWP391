using Microsoft.AspNetCore.Http;

namespace SWP391.DTOs
{
    public class PhotoUploadDto
    {
        public IFormFile File { get; set; }
    }
} 