using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusSinhVien.API.Data;
using NexusSinhVien.API.Models.Entities;

namespace NexusSinhVien.API.Controllers
{
    [Route("api/phieu-giam-gia")]
    [ApiController]
    public class PhieuGiamGiaController : ControllerBase
    {
        private readonly NexusDbContext _context;

        public PhieuGiamGiaController(NexusDbContext context)
        {
            _context = context;
        }

        // GET: api/phieu-giam-gia/cua-toi
        [HttpGet("cua-toi")]
        [Authorize]
        public async Task<IActionResult> PhieuCuaToi()
        {
            var maStr = User.FindFirst("MaNguoiDung")?.Value;
            if (!int.TryParse(maStr, out int ma)) return Unauthorized();

            var ds = await _context.PhieuGiamGias
                .Include(p => p.UuDai)
                .ThenInclude(u => u!.CoSo)
                .Where(p => p.MaNguoiDung == ma)
                .OrderByDescending(p => p.NgayTao)
                .Select(p => new
                {
                    p.MaPhieu,
                    p.MaCode,
                    p.TrangThai,
                    p.NgayHetHan,
                    p.NgayTao,
                    TenUuDai = p.UuDai != null ? p.UuDai.TieuDe : "",
                    TenCoSo = p.UuDai != null && p.UuDai.CoSo != null
                              ? p.UuDai.CoSo.TenCoSo : ""
                })
                .ToListAsync();

            return Ok(ds);
        }

        // POST: api/phieu-giam-gia/doi-diem
        [HttpPost("doi-diem")]
        [Authorize]
        public async Task<IActionResult> DoiDiem([FromBody] DoiDiemRequest req)
        {
            var maStr = User.FindFirst("MaNguoiDung")?.Value;
            if (!int.TryParse(maStr, out int maNguoiDung)) return Unauthorized();

            // Kiểm tra số điểm cần đổi
            int soDiemCan = req.SoDiem;

            var nd = await _context.NguoiDungs.FindAsync(maNguoiDung);
            if (nd == null) return NotFound();

            if (nd.DiemTichLuy < soDiemCan)
                return BadRequest(new { message = $"Bạn chỉ có {nd.DiemTichLuy} điểm, cần {soDiemCan} điểm" });

            // Tạo mã voucher ngẫu nhiên
            var maCode = "SV" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

            // Trừ điểm
            nd.DiemTichLuy -= soDiemCan;

            // Tạo voucher
            var phieu = new PhieuGiamGia
            {
                MaNguoiDung = maNguoiDung,
                MaUuDai = req.MaUuDai,
                MaCode = maCode,
                SoDiemDaDung = soDiemCan,
                TrangThai = "chua_dung",
                NgayHetHan = DateTime.Now.AddDays(30)
            };

            _context.PhieuGiamGias.Add(phieu);

            // Ghi log điểm
            _context.DiemThuongs.Add(new DiemThuong
            {
                MaNguoiDung = maNguoiDung,
                SoDiem = -soDiemCan,
                HanhDong = "Đổi điểm lấy voucher",
                MaThamChieu = phieu.MaPhieu
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Đổi điểm thành công!",
                maCode = maCode,
                diemConLai = nd.DiemTichLuy
            });
        }
    }

    public record DoiDiemRequest(int MaUuDai, int SoDiem);
}