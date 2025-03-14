using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace SWP391.DTOs
{
    public class MultiPhotoUploadDto
    {
        public List<IFormFile> Files { get; set; }
    }
} 