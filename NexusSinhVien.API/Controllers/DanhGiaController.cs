using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusSinhVien.API.Data;
using NexusSinhVien.API.Models.Entities;

namespace NexusSinhVien.API.Controllers
{
    [Route("api/danh-gia")]
    [ApiController]
    public class DanhGiaController : ControllerBase
    {
        private readonly NexusDbContext _context;

        public DanhGiaController(NexusDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET: api/danh-gia/co-so/{maCoSo}
        // Ai cũng có thể xem đánh giá của cơ sở
        // =====================================================
        [HttpGet("co-so/{maCoSo}")]
        public async Task<IActionResult> LayTheoCoSo(int maCoSo)
        {
            var ds = await _context.DanhGias
                .Include(d => d.NguoiDung)
                .Where(d => d.MaCoSo == maCoSo)
                .OrderByDescending(d => d.NgayDanhGia)
                .Select(d => new
                {
                    d.MaDanhGia,
                    d.DiemDanhGia,
                    d.BinhLuan,
                    d.NgayDanhGia,
                    TenNguoiDung = d.NguoiDung != null ? d.NguoiDung.HoTen : "Ẩn danh"
                })
                .ToListAsync();

            return Ok(ds);
        }

        // =====================================================
        // GET: api/danh-gia/cua-toi
        // ✅ CHỈ SINH VIÊN mới có đánh giá của mình
        // =====================================================
        [HttpGet("cua-toi")]
        [Authorize(Roles = "sinh_vien")] // SỬA Ở ĐÂY
        public async Task<IActionResult> DanhGiaCuaToi()
        {
            var maStr = User.FindFirst("MaNguoiDung")?.Value;
            if (!int.TryParse(maStr, out int maNguoiDung))
                return Unauthorized(new { message = "Chưa đăng nhập" });

            var ds = await _context.DanhGias
                .Include(d => d.CoSo)
                .Where(d => d.MaNguoiDung == maNguoiDung)
                .OrderByDescending(d => d.NgayDanhGia)
                .Select(d => new
                {
                    d.MaDanhGia,
                    d.MaCoSo,
                    d.DiemDanhGia,
                    d.BinhLuan,
                    d.NgayDanhGia,
                    TenCoSo = d.CoSo != null ? d.CoSo.TenCoSo : "Không xác định"
                })
                .ToListAsync();

            return Ok(ds);
        }

        // =====================================================
        // POST: api/danh-gia
        // ✅ CHỈ SINH VIÊN MỚI ĐƯỢC ĐÁNH GIÁ
        // =====================================================
        [HttpPost]
        [Authorize(Roles = "sinh_vien")] // SỬA Ở ĐÂY
        public async Task<IActionResult> ThemMoi([FromBody] DanhGiaRequest req)
        {
            try
            {
                var maStr = User.FindFirst("MaNguoiDung")?.Value;
                if (!int.TryParse(maStr, out int maNguoiDung))
                    return Unauthorized(new { message = "Chưa đăng nhập" });

                // 1. Kiểm tra cơ sở tồn tại
                var coSo = await _context.CoSoKinhDoanhs.FindAsync(req.MaCoSo);
                if (coSo == null)
                    return NotFound(new { message = "Không tìm thấy cơ sở" });

                // 2. Kiểm tra đã đánh giá chưa
                var daDanhGia = await _context.DanhGias
                    .AnyAsync(d => d.MaNguoiDung == maNguoiDung && d.MaCoSo == req.MaCoSo);
                if (daDanhGia)
                    return BadRequest(new { message = "Bạn đã đánh giá cơ sở này rồi" });

                // 3. Thêm đánh giá mới
                var dg = new DanhGia
                {
                    MaNguoiDung = maNguoiDung,
                    MaCoSo = req.MaCoSo,
                    DiemDanhGia = req.DiemDanhGia,
                    BinhLuan = req.BinhLuan,
                    NgayDanhGia = DateTime.Now
                };
                _context.DanhGias.Add(dg);

                // 4. Cộng điểm cho user
                var nguoiDung = await _context.NguoiDungs.FindAsync(maNguoiDung);
                if (nguoiDung != null)
                    nguoiDung.DiemTichLuy += 10;

                // 5. Lưu đánh giá + điểm user TRƯỚC
                await _context.SaveChangesAsync();

                // 6. Tính lại điểm trung bình SAU KHI đã có đánh giá mới
                var danhSachDiem = await _context.DanhGias
                    .Where(d => d.MaCoSo == req.MaCoSo)
                    .Select(d => (double)d.DiemDanhGia)
                    .ToListAsync();

                double trungBinh = danhSachDiem.Any()
                    ? danhSachDiem.Average()
                    : (double)req.DiemDanhGia;

                coSo.DanhGiaTrungBinh = (decimal)Math.Round(trungBinh, 2);

                // 7. Lưu lại điểm trung bình
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Đánh giá thành công! Bạn được +10 điểm",
                    maDanhGia = dg.MaDanhGia,
                    diemTichLuy = nguoiDung?.DiemTichLuy ?? 0
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ LỖI ĐÁNH GIÁ: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Có lỗi xảy ra khi gửi đánh giá",
                    detail = ex.Message
                });
            }
        }
    }

    // DTO
    public record DanhGiaRequest(int MaCoSo, byte DiemDanhGia, string? BinhLuan);
}