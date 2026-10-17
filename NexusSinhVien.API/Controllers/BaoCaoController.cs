using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusSinhVien.API.Data;
using NexusSinhVien.API.Models.Entities;

namespace NexusSinhVien.API.Controllers
{
    [Route("api/bao-cao")]
    [ApiController]
    public class BaoCaoController : ControllerBase
    {
        private readonly NexusDbContext _context;

        public BaoCaoController(NexusDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // POST: api/bao-cao
        // Người dùng gửi báo cáo vi phạm
        // =====================================================
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GuiBaoCao([FromBody] BaoCaoRequest req)
        {
            var maStr = User.FindFirst("MaNguoiDung")?.Value;
            if (!int.TryParse(maStr, out int maNguoiDung))
                return Unauthorized(new { message = "Chưa đăng nhập" });

            // Kiểm tra loại đối tượng hợp lệ
            if (req.LoaiDoiTuong != "co_so"
                && req.LoaiDoiTuong != "dich_vu"
                && req.LoaiDoiTuong != "danh_gia")
            {
                return BadRequest(new { message = "Loại đối tượng không hợp lệ" });
            }

            // Kiểm tra lý do không được trống
            if (string.IsNullOrWhiteSpace(req.LyDo))
                return BadRequest(new { message = "Vui lòng nhập lý do báo cáo" });

            // Giới hạn lý do tối đa 1000 ký tự
            if (req.LyDo.Length > 1000)
                return BadRequest(new { message = "Lý do không được vượt quá 1000 ký tự" });

            var bc = new BaoCao
            {
                MaNguoiDung = maNguoiDung,
                LoaiDoiTuong = req.LoaiDoiTuong,
                MaDoiTuong = req.MaDoiTuong,
                LyDo = req.LyDo.Trim(),
                TrangThai = "cho_xu_ly",
                NgayTao = DateTime.Now
            };

            _context.BaoCaos.Add(bc);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Đã gửi báo cáo. Cảm ơn bạn đã đóng góp!",
                maBaoCao = bc.MaBaoCao
            });
        }

        // =====================================================
        // GET: api/bao-cao/cua-toi
        // Xem danh sách báo cáo của chính mình
        // =====================================================
        [HttpGet("cua-toi")]
        [Authorize]
        public async Task<IActionResult> BaoCaoCuaToi()
        {
            var maStr = User.FindFirst("MaNguoiDung")?.Value;
            if (!int.TryParse(maStr, out int maNguoiDung))
                return Unauthorized();

            var ds = await _context.BaoCaos
                .Where(b => b.MaNguoiDung == maNguoiDung)
                .OrderByDescending(b => b.NgayTao)
                .Select(b => new
                {
                    b.MaBaoCao,
                    b.LoaiDoiTuong,
                    b.MaDoiTuong,
                    b.LyDo,
                    b.TrangThai,
                    b.NgayTao
                })
                .ToListAsync();

            return Ok(ds);
        }
    }

    // =====================================================
    // DTO
    // =====================================================
    public record BaoCaoRequest(
        string LoaiDoiTuong,
        int MaDoiTuong,
        string LyDo);
}