using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using lamlai.Models;
using SWP391.DTOs;
using SWP391.Services;

namespace SWP391.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly TestContext _context;
        private readonly IPhotoService _photoService;
        private const int MAX_PHOTOS_PER_PRODUCT = 5;

        public PhotosController(TestContext context, IPhotoService photoService)
        {
            _context = context;
            _photoService = photoService;
        }

        // GET: api/Photos/product
        [HttpGet("product")]
        public async Task<ActionResult<IEnumerable<PhotoDto>>> GetAllProductPhotos()
        {
            try
            {
                var photos = await _context.ProductImages
                    .OrderBy(p => p.ProductId)
                    .ThenBy(p => p.DisplayOrder)
                    .ToListAsync();

                var photoDtos = photos.Select(p => new PhotoDto
                {
                    ImageID = p.ImageID,
                    ProductId = p.ProductId,
                    ImgUrl = p.ImgUrl,
                    DisplayOrder = p.DisplayOrder
                }).ToList();

                return photoDtos;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy danh sách ảnh: {ex.Message}");
            }
        }

        // GET: api/Photos/product/{productId}
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<PhotoDto>>> GetProductPhotos(int productId)
        {
            // Kiểm tra sản phẩm tồn tại bằng cách truy vấn chỉ ProductId
            var productExists = await _context.Products
                .AnyAsync(p => p.ProductId == productId);
            
            if (!productExists)
                return NotFound("Không tìm thấy sản phẩm");

            var photos = await _context.ProductImages
                .Where(p => p.ProductId == productId)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();

            var photoDtos = photos.Select(p => new PhotoDto
            {
                ImageID = p.ImageID,
                ProductId = p.ProductId,
                ImgUrl = p.ImgUrl,
                DisplayOrder = p.DisplayOrder
            }).ToList();

            return photoDtos;
        }

        // POST: api/Photos/upload
        [HttpPost("upload")]
        public async Task<ActionResult<PhotoDto>> UploadPhoto([FromForm] PhotoUploadDto photoUploadDto)
        {
            var result = await _photoService.AddPhotoAsync(photoUploadDto.File);

            if (result.Error != null)
                return BadRequest(result.Error.Message);

            var photo = new ProductImage
            {
                ImgUrl = result.SecureUrl.AbsoluteUri,
                DisplayOrder = 0
            };

            return new PhotoDto
            {
                ImageID = photo.ImageID,
                ProductId = photo.ProductId,
                ImgUrl = photo.ImgUrl,
                DisplayOrder = photo.DisplayOrder
            };
        }

        // POST: api/Photos/upload/product/{productId}
        [HttpPost("upload/product/{productId}")]
        public async Task<ActionResult<PhotoDto>> UploadProductPhoto(int productId, [FromForm] PhotoUploadDto photoUploadDto)
        {
            // Kiểm tra sản phẩm tồn tại bằng cách truy vấn chỉ ProductId
            var productExists = await _context.Products
                .AnyAsync(p => p.ProductId == productId);
            
            if (!productExists)
                return NotFound("Không tìm thấy sản phẩm");

            // Kiểm tra số lượng ảnh hiện tại
            var currentPhotosCount = await _context.ProductImages.CountAsync(p => p.ProductId == productId);
            if (currentPhotosCount >= MAX_PHOTOS_PER_PRODUCT)
                return BadRequest($"Sản phẩm chỉ được phép có tối đa {MAX_PHOTOS_PER_PRODUCT} ảnh");

            var result = await _photoService.AddPhotoAsync(photoUploadDto.File);

            if (result.Error != null)
                return BadRequest(result.Error.Message);

            // Tìm DisplayOrder lớn nhất hiện tại
            var maxDisplayOrder = await _context.ProductImages
                .Where(p => p.ProductId == productId)
                .Select(p => (int?)p.DisplayOrder)
                .MaxAsync() ?? 0;

            var photo = new ProductImage
            {
                ProductId = productId,
                ImgUrl = result.SecureUrl.AbsoluteUri,
                DisplayOrder = maxDisplayOrder + 1
            };

            _context.ProductImages.Add(photo);
            await _context.SaveChangesAsync();

            return new PhotoDto
            {
                ImageID = photo.ImageID,
                ProductId = photo.ProductId,
                ImgUrl = photo.ImgUrl,
                DisplayOrder = photo.DisplayOrder
            };
        }

        // POST: api/Photos/upload-multiple/product/{productId}
        [HttpPost("upload-multiple/product/{productId}")]
        public async Task<ActionResult<IEnumerable<PhotoDto>>> UploadMultipleProductPhotos(int productId, [FromForm] MultiPhotoUploadDto multiPhotoUploadDto)
        {
            if (multiPhotoUploadDto.Files == null || !multiPhotoUploadDto.Files.Any())
                return BadRequest("Không có file nào được chọn");

            // Kiểm tra sản phẩm tồn tại bằng cách truy vấn chỉ ProductId
            var productExists = await _context.Products
                .AnyAsync(p => p.ProductId == productId);
            
            if (!productExists)
                return NotFound("Không tìm thấy sản phẩm");

            // Kiểm tra số lượng ảnh hiện tại
            var currentPhotosCount = await _context.ProductImages.CountAsync(p => p.ProductId == productId);
            if (currentPhotosCount + multiPhotoUploadDto.Files.Count > MAX_PHOTOS_PER_PRODUCT)
                return BadRequest($"Sản phẩm chỉ được phép có tối đa {MAX_PHOTOS_PER_PRODUCT} ảnh. Hiện tại đã có {currentPhotosCount} ảnh.");

            var photoDtos = new List<PhotoDto>();
            var maxDisplayOrder = await _context.ProductImages
                .Where(p => p.ProductId == productId)
                .Select(p => (int?)p.DisplayOrder)
                .MaxAsync() ?? 0;

            foreach (var file in multiPhotoUploadDto.Files)
            {
                var result = await _photoService.AddPhotoAsync(file);

                if (result.Error != null)
                    continue; // Bỏ qua ảnh lỗi và tiếp tục với ảnh khác

                maxDisplayOrder++;
                var photo = new ProductImage
                {
                    ProductId = productId,
                    ImgUrl = result.SecureUrl.AbsoluteUri,
                    DisplayOrder = maxDisplayOrder
                };

                _context.ProductImages.Add(photo);
                await _context.SaveChangesAsync(); // Lưu từng ảnh để có ImageID

                photoDtos.Add(new PhotoDto
                {
                    ImageID = photo.ImageID,
                    ProductId = photo.ProductId,
                    ImgUrl = photo.ImgUrl,
                    DisplayOrder = photo.DisplayOrder
                });

                currentPhotosCount++;
            }

            return photoDtos;
        }

        // PUT: api/Photos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePhoto(int id, [FromForm] PhotoUpdateDto photoUpdateDto)
        {
            try
            {
                // Kiểm tra validation
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Kiểm tra file ảnh
                if (!PhotoUpdateDto.IsValidImage(photoUpdateDto.File))
                    return BadRequest("File ảnh không hợp lệ. Kích thước tối đa 5MB, định dạng: jpg, jpeg, png, gif");

                // Tìm ảnh cần cập nhật
                var photo = await _context.ProductImages.FindAsync(id);
                if (photo == null)
                    return NotFound("Không tìm thấy ảnh");

                // Kiểm tra số lượng ảnh của sản phẩm
                var productPhotosCount = await _context.ProductImages
                    .CountAsync(p => p.ProductId == photo.ProductId);

                if (productPhotosCount > 5)
                    return BadRequest("Sản phẩm đã đạt giới hạn tối đa 5 ảnh");

                // Upload ảnh mới
                var uploadResult = await _photoService.AddPhotoAsync(photoUpdateDto.File);
                if (uploadResult.Error != null)
                    return BadRequest(uploadResult.Error.Message);

                // Cập nhật thông tin ảnh
                photo.ImgUrl = uploadResult.SecureUrl.AbsoluteUri;
                photo.DisplayOrder = photoUpdateDto.DisplayOrder;

                await _context.SaveChangesAsync();

                return Ok(new PhotoDto
                {
                    ImageID = photo.ImageID,
                    ProductId = photo.ProductId,
                    ImgUrl = photo.ImgUrl,
                    DisplayOrder = photo.DisplayOrder
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi cập nhật ảnh: {ex.Message}");
            }
        }

        // DELETE: api/Photos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int id)
        {
            var photo = await _context.ProductImages.FindAsync(id);
            
            if (photo == null)
                return NotFound("Không tìm thấy ảnh");

            _context.ProductImages.Remove(photo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Photos/product/{productId}
        [HttpDelete("product/{productId}")]
        public async Task<IActionResult> DeleteAllProductPhotos(int productId)
        {
            // Kiểm tra sản phẩm tồn tại bằng cách truy vấn chỉ ProductId
            var productExists = await _context.Products
                .AnyAsync(p => p.ProductId == productId);
            
            if (!productExists)
                return NotFound("Không tìm thấy sản phẩm");

            var photos = await _context.ProductImages
                .Where(p => p.ProductId == productId)
                .ToListAsync();

            foreach (var photo in photos)
            {
                _context.ProductImages.Remove(photo);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Photos/reorder
        [HttpPut("reorder")]
        public async Task<IActionResult> ReorderPhotos([FromBody] List<PhotoDto> photoDtos)
        {
            try
            {
                if (photoDtos == null || !photoDtos.Any())
                    return BadRequest("Danh sách ảnh không được để trống");

                // Kiểm tra số lượng ảnh
                if (photoDtos.Count > 5)
                    return BadRequest("Số lượng ảnh không được vượt quá 5");

                // Kiểm tra DisplayOrder hợp lệ
                if (photoDtos.Any(p => p.DisplayOrder < 0 || p.DisplayOrder > 4))
                    return BadRequest("Thứ tự hiển thị phải từ 0 đến 4");

                // Kiểm tra trùng lặp DisplayOrder
                var displayOrders = photoDtos.Select(p => p.DisplayOrder).ToList();
                if (displayOrders.Distinct().Count() != displayOrders.Count)
                    return BadRequest("Thứ tự hiển thị không được trùng lặp");

                foreach (var photoDto in photoDtos)
                {
                    var photo = await _context.ProductImages.FindAsync(photoDto.ImageID);
                    if (photo == null)
                        return NotFound($"Không tìm thấy ảnh với ID: {photoDto.ImageID}");

                    photo.DisplayOrder = photoDto.DisplayOrder;
                }

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi sắp xếp lại ảnh: {ex.Message}");
            }
        }
    }
} 