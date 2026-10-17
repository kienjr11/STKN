using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusSinhVien.API.Data;
using NexusSinhVien.API.Models.Entities;

namespace NexusSinhVien.API.Controllers
{
    [Route("api/diem")]
    [ApiController]
    public class DiemController : ControllerBase
    {
        private readonly NexusDbContext _context;

        public DiemController(NexusDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET: api/diem/lich-su
        // Lấy điểm tích lũy và ghi chú
        // =====================================================
        [HttpGet("lich-su")]
        [Authorize]
        public async Task<IActionResult> LichSu()
        {
            var maStr = User.FindFirst("MaNguoiDung")?.Value;
            if (!int.TryParse(maStr, out int ma))
                return Unauthorized();

            var nd = await _context.NguoiDungs.FindAsync(ma);
            if (nd == null) return NotFound();

            return Ok(new
            {
                diemTichLuy = nd.DiemTichLuy,
                ghiChu = "Điểm được tích lũy từ: đăng ký (+50), xác thực (+100), đánh giá (+10), chia sẻ (+20/ngày)"
            });
        }

        // =====================================================
        // GET: api/diem/chi-tiet
        // Lấy lịch sử chi tiết các giao dịch điểm
        // =====================================================
        [HttpGet("chi-tiet")]
        [Authorize]
        public async Task<IActionResult> ChiTiet()
        {
            var maStr = User.FindFirst("MaNguoiDung")?.Value;
            if (!int.TryParse(maStr, out int ma))
                return Unauthorized();

            var ds = await _context.DiemThuongs
                .Where(d => d.MaNguoiDung == ma)
                .OrderByDescending(d => d.NgayTao)
                .Select(d => new
                {
                    d.MaDiem,
                    d.SoDiem,
                    d.HanhDong,
                    d.NgayTao
                })
                .ToListAsync();

            return Ok(ds);
        }

        // =====================================================
        // POST: api/diem/chia-se
        // Chia sẻ nền tảng → +20 điểm (mỗi ngày 1 lần)
        // =====================================================
        [HttpPost("chia-se")]
        [Authorize]
        public async Task<IActionResult> ChiaSe()
        {
            var maStr = User.FindFirst("MaNguoiDung")?.Value;
            if (!int.TryParse(maStr, out int ma))
                return Unauthorized();

            var nd = await _context.NguoiDungs.FindAsync(ma);
            if (nd == null) return NotFound();

            // Kiểm tra đã chia sẻ hôm nay chưa
            var homNay = DateTime.Today;
            var ngayMai = homNay.AddDays(1);

            var daChiaSe = await _context.DiemThuongs
                .AnyAsync(d => d.MaNguoiDung == ma
                            && d.HanhDong == "Chia sẻ nền tảng"
                            && d.NgayTao >= homNay
                            && d.NgayTao < ngayMai);

            if (daChiaSe)
                return BadRequest(new
                {
                    message = "Bạn đã chia sẻ hôm nay rồi. Quay lại vào ngày mai nhé!"
                });

            // Cộng điểm
            nd.DiemTichLuy += 20;

            // Ghi log
            _context.DiemThuongs.Add(new DiemThuong
            {
                MaNguoiDung = ma,
                SoDiem = 20,
                HanhDong = "Chia sẻ nền tảng",
                NgayTao = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Chia sẻ thành công! Bạn được +20 điểm",
                diemTichLuy = nd.DiemTichLuy,
                diemCong = 20
            });
        }

        // =====================================================
        // POST: api/diem/dang-nhap-hang-ngay
        // Điểm danh hàng ngày → +5 điểm
        // =====================================================
        [HttpPost("dang-nhap-hang-ngay")]
        [Authorize]
        public async Task<IActionResult> DiemDanhHangNgay()
        {
            var maStr = User.FindFirst("MaNguoiDung")?.Value;
            if (!int.TryParse(maStr, out int ma))
                return Unauthorized();

            var nd = await _context.NguoiDungs.FindAsync(ma);
            if (nd == null) return NotFound();

            var homNay = DateTime.Today;
            var ngayMai = homNay.AddDays(1);

            var daDiemDanh = await _context.DiemThuongs
                .AnyAsync(d => d.MaNguoiDung == ma
                            && d.HanhDong == "Điểm danh hàng ngày"
                            && d.NgayTao >= homNay
                            && d.NgayTao < ngayMai);

            if (daDiemDanh)
                return BadRequest(new { message = "Bạn đã điểm danh hôm nay rồi!" });

            nd.DiemTichLuy += 5;

            _context.DiemThuongs.Add(new DiemThuong
            {
                MaNguoiDung = ma,
                SoDiem = 5,
                HanhDong = "Điểm danh hàng ngày",
                NgayTao = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Điểm danh thành công! Bạn được +5 điểm",
                diemTichLuy = nd.DiemTichLuy,
                diemCong = 5
            });
        }
    }
}