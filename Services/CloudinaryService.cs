using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SWP391.Models;

namespace SWP391.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> cloudinarySettings, IConfiguration configuration)
        {
            var cloudName = cloudinarySettings.Value.CloudName;
            var apiKey = cloudinarySettings.Value.ApiKey;
            var apiSecret = cloudinarySettings.Value.ApiSecret;

            // Kiểm tra xem có thông tin cấu hình Cloudinary không
            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new ArgumentException("Cloudinary configuration is missing. Please check appsettings.json.");
            }

            // Log thông tin cấu hình (không bao gồm apiSecret vì lý do bảo mật)
            Console.WriteLine($"Cloudinary Configuration - CloudName: {cloudName}, ApiKey: {apiKey}");

            // Khởi tạo Account với các thông tin đã lấy
            var account = new Account(cloudName, apiKey, apiSecret);
            
            // Khởi tạo đối tượng Cloudinary
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadBase64Image(string base64String, string folder = "messages")
        {
            try
            {
                if (string.IsNullOrEmpty(base64String))
                    return null;

                // Chuẩn bị dữ liệu base64
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription($"data:{GetMimeType(base64String)};base64,{GetBase64Data(base64String)}"),
                    Folder = folder,
                    UseFilename = true,
                    UniqueFilename = true
                };

                // Tải lên Cloudinary
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    throw new Exception($"Lỗi tải ảnh lên Cloudinary: {uploadResult.Error.Message}");
                }

                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi tải ảnh lên Cloudinary: {ex.Message}", ex);
            }
        }

        public async Task<string> UploadFile(IFormFile file, string folder = "messages")
        {
            try
            {
                if (file == null || file.Length == 0)
                    return null;

                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = folder,
                        UseFilename = true,
                        UniqueFilename = true
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                    if (uploadResult.Error != null)
                    {
                        throw new Exception($"Lỗi tải ảnh lên Cloudinary: {uploadResult.Error.Message}");
                    }

                    return uploadResult.SecureUrl.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi tải ảnh lên Cloudinary: {ex.Message}", ex);
            }
        }

        public async Task DeleteImage(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return;

                // Lấy public ID từ URL
                var publicId = GetPublicIdFromUrl(imageUrl);
                if (string.IsNullOrEmpty(publicId))
                    return;

                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);

                if (result.Error != null)
                {
                    throw new Exception($"Lỗi xóa ảnh từ Cloudinary: {result.Error.Message}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi xóa ảnh từ Cloudinary: {ex.Message}", ex);
            }
        }

        private string GetBase64Data(string base64String)
        {
            // Nếu chuỗi đã có định dạng "data:image/xyz;base64,", cần trích xuất phần dữ liệu
            if (base64String.Contains(";base64,"))
            {
                return base64String.Split(";base64,")[1];
            }
            return base64String;
        }

        private string GetMimeType(string base64String)
        {
            // Trích xuất MIME type nếu có trong chuỗi
            if (base64String.Contains("data:") && base64String.Contains(";base64,"))
            {
                var mimeType = base64String.Split(';')[0].Split(':')[1];
                return mimeType;
            }
            // Mặc định trả về MIME type là image/jpeg
            return "image/jpeg";
        }

        private string GetPublicIdFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            try
            {
                // Cloudinary URLs có dạng: https://res.cloudinary.com/cloud-name/image/upload/v1234567890/folder/filename.ext
                Uri uri = new Uri(url);
                string path = uri.AbsolutePath;
                
                // Bỏ qua phần "/image/upload/" và phiên bản (v1234567890)
                int startIndex = path.IndexOf("/upload/");
                if (startIndex < 0)
                    return null;

                string publicIdWithVersion = path.Substring(startIndex + 8); // +8 để bỏ qua "/upload/"
                
                // Bỏ qua phần phiên bản nếu có (v1234567890/)
                int versionEndIndex = publicIdWithVersion.IndexOf("/");
                if (versionEndIndex > 0 && publicIdWithVersion.StartsWith("v"))
                {
                    publicIdWithVersion = publicIdWithVersion.Substring(versionEndIndex + 1);
                }
                
                // Bỏ phần mở rộng file (.jpg, .png, etc.)
                int extIndex = publicIdWithVersion.LastIndexOf(".");
                if (extIndex > 0)
                {
                    publicIdWithVersion = publicIdWithVersion.Substring(0, extIndex);
                }
                
                return publicIdWithVersion;
            }
            catch
            {
                return null;
            }
        }
    }
} 