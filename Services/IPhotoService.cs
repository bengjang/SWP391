using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using SWP391.DTOs;
using System.Threading.Tasks;

namespace SWP391.Services
{
    public interface IPhotoService
    {
        Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
        Task<DeletionResult> DeletePhotoAsync(string publicId);
    }
} 