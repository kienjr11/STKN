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

        // GET: api/danh-gia/co-so/{maCoSo}
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

        // POST: api/danh-gia
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ThemMoi([FromBody] DanhGiaRequest req)
        {
            var maStr = User.FindFirst("MaNguoiDung")?.Value;
            if (!int.TryParse(maStr, out int maNguoiDung))
                return Unauthorized(new { message = "Chưa đăng nhập" });

            var coSo = await _context.CoSoKinhDoanhs.FindAsync(req.MaCoSo);
            if (coSo == null)
                return NotFound(new { message = "Không tìm thấy cơ sở" });

            var daDanhGia = await _context.DanhGias
                .AnyAsync(d => d.MaNguoiDung == maNguoiDung && d.MaCoSo == req.MaCoSo);
            if (daDanhGia)
                return BadRequest(new { message = "Bạn đã đánh giá cơ sở này rồi" });

            var dg = new DanhGia
            {
                MaNguoiDung = maNguoiDung,
                MaCoSo = req.MaCoSo,
                DiemDanhGia = req.DiemDanhGia,
                BinhLuan = req.BinhLuan,
                NgayDanhGia = DateTime.Now
            };
            _context.DanhGias.Add(dg);

            // Cập nhật điểm trung bình
            var trungBinh = await _context.DanhGias
                .Where(d => d.MaCoSo == req.MaCoSo)
                .AverageAsync(d => (double)d.DiemDanhGia);

            coSo.DanhGiaTrungBinh = (decimal)Math.Round(trungBinh, 2);

            // Cộng điểm cho người dùng
            var nguoiDung = await _context.NguoiDungs.FindAsync(maNguoiDung);
            if (nguoiDung != null)
                nguoiDung.DiemTichLuy += 10;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Đánh giá thành công! Bạn được +10 điểm",
                maDanhGia = dg.MaDanhGia,
                diemTichLuy = nguoiDung?.DiemTichLuy ?? 0
            });
        }
    }

    public record DanhGiaRequest(int MaCoSo, byte DiemDanhGia, string? BinhLuan);
}