using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NexusSinhVien.API.Controllers
{
    [Route("api/upload")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // POST: api/upload/anh
        [HttpPost("anh")]
        [Authorize]
        public async Task<IActionResult> UploadAnh(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Chưa chọn file" });

            // Kiểm tra định dạng
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(ext))
                return BadRequest(new { message = "Chỉ chấp nhận file ảnh (jpg, png, gif, webp)" });

            // Giới hạn 5MB
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "File không được vượt quá 5MB" });

            // Tạo tên file unique
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var uploadPath = Path.Combine(_env.WebRootPath, "uploads");

            // Tạo thư mục nếu chưa có
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);

            // Lưu file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/uploads/{fileName}";
            return Ok(new { message = "Upload thành công", url = url });
        }
    }
}