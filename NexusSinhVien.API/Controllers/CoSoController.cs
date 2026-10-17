using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusSinhVien.API.Data;
using NexusSinhVien.API.Models.Entities;

namespace NexusSinhVien.API.Controllers
{
    [Route("api/co-so")]
    [ApiController]
    public class CoSoController : ControllerBase
    {
        private readonly NexusDbContext _context;

        public CoSoController(NexusDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET: api/co-so
        // Lấy danh sách cơ sở với bộ lọc nâng cao
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> LayTatCa(
            [FromQuery] int? maDanhMuc,
            [FromQuery] string? tuKhoa,
            [FromQuery] decimal? giaMin,
            [FromQuery] decimal? giaMax,
            [FromQuery] double? danhGiaMin,
            [FromQuery] double? viDo,
            [FromQuery] double? kinhDo,
            [FromQuery] double? banKinh) // km
        {
            var query = _context.CoSoKinhDoanhs
                .Include(c => c.DanhMuc)
                .Include(c => c.DanhSachDichVu)
                .Where(c => c.TrangThai == "hoat_dong");

            // Lọc theo danh mục
            if (maDanhMuc.HasValue)
                query = query.Where(c => c.MaDanhMuc == maDanhMuc);

            // Lọc theo từ khóa (tên hoặc mô tả)
            if (!string.IsNullOrEmpty(tuKhoa))
                query = query.Where(c => c.TenCoSo.Contains(tuKhoa)
                                      || (c.MoTa != null && c.MoTa.Contains(tuKhoa)));

            // Lọc theo đánh giá tối thiểu
            if (danhGiaMin.HasValue)
                query = query.Where(c => c.DanhGiaTrungBinh >= (decimal)danhGiaMin);

            // Lọc theo khoảng giá (dựa vào dịch vụ có giá trong khoảng)
            if (giaMin.HasValue || giaMax.HasValue)
            {
                query = query.Where(c => c.DanhSachDichVu != null
                    && c.DanhSachDichVu.Any(d =>
                        (!giaMin.HasValue || d.Gia >= giaMin) &&
                        (!giaMax.HasValue || d.Gia <= giaMax)));
            }

            var ds = await query.ToListAsync();

            // Lọc theo khoảng cách (Haversine)
            if (viDo.HasValue && kinhDo.HasValue && banKinh.HasValue)
            {
                ds = ds.Where(c =>
                {
                    if (!c.ViDo.HasValue || !c.KinhDo.HasValue) return false;
                    var distance = TinhKhoangCach(
                        viDo.Value, kinhDo.Value,
                        (double)c.ViDo.Value, (double)c.KinhDo.Value);
                    return distance <= banKinh.Value;
                }).ToList();
            }

            return Ok(ds.OrderByDescending(c => c.DanhGiaTrungBinh));
        }

        // =====================================================
        // GET: api/co-so/{id}
        // Lấy chi tiết 1 cơ sở
        // =====================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> LayTheoId(int id)
        {
            var cs = await _context.CoSoKinhDoanhs
                .Include(c => c.DanhMuc)
                .Include(c => c.DanhSachDichVu)
                .Include(c => c.DanhSachUuDai)
                .FirstOrDefaultAsync(c => c.MaCoSo == id);

            if (cs == null) return NotFound(new { message = "Không tìm thấy cơ sở" });
            return Ok(cs);
        }

        // =====================================================
        // API CHO ĐỐI TÁC
        // =====================================================

        // GET: api/co-so/cua-toi
        [HttpGet("cua-toi")]
        [Authorize]
        public async Task<IActionResult> CoSoCuaToi()
        {
            var maStr = User.FindFirst("MaNguoiDung")?.Value;
            if (!int.TryParse(maStr, out int maNguoiDung))
                return Unauthorized();

            var ds = await _context.CoSoKinhDoanhs
                .Include(c => c.DanhMuc)
                .Where(c => c.MaChuSoHuu == maNguoiDung)
                .OrderByDescending(c => c.NgayTao)
                .ToListAsync();

            return Ok(ds);
        }

        // POST: api/co-so/dang-ky
        [HttpPost("dang-ky")]
        [Authorize]
        public async Task<IActionResult> DangKy([FromBody] DangKyCoSoRequest req)
        {
            var maStr = User.FindFirst("MaNguoiDung")?.Value;
            if (!int.TryParse(maStr, out int maNguoiDung))
                return Unauthorized();

            var cs = new CoSoKinhDoanh
            {
                MaChuSoHuu = maNguoiDung,
                TenCoSo = req.TenCoSo,
                MaDanhMuc = req.MaDanhMuc,
                MoTa = req.MoTa,
                DiaChi = req.DiaChi,
                ViDo = req.ViDo,
                KinhDo = req.KinhDo,
                SoDienThoai = req.SoDienThoai,
                GioMoCua = req.GioMoCua,
                TrangThai = "cho_duyet",
                DanhGiaTrungBinh = 0
            };

            _context.CoSoKinhDoanhs.Add(cs);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Đăng ký thành công! Vui lòng chờ quản trị viên duyệt.",
                maCoSo = cs.MaCoSo
            });
        }

        // PUT: api/co-so/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> CapNhat(int id, [FromBody] DangKyCoSoRequest req)
        {
            var maStr = User.FindFirst("MaNguoiDung")?.Value;
            if (!int.TryParse(maStr, out int maNguoiDung))
                return Unauthorized();

            var cs = await _context.CoSoKinhDoanhs.FindAsync(id);
            if (cs == null) return NotFound();

            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (cs.MaChuSoHuu != maNguoiDung && role != "quan_tri")
                return Forbid();

            cs.TenCoSo = req.TenCoSo;
            cs.MaDanhMuc = req.MaDanhMuc;
            cs.MoTa = req.MoTa;
            cs.DiaChi = req.DiaChi;
            cs.ViDo = req.ViDo;
            cs.KinhDo = req.KinhDo;
            cs.SoDienThoai = req.SoDienThoai;
            cs.GioMoCua = req.GioMoCua;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công" });
        }

        // =====================================================
        // HÀM TÍNH KHOẢNG CÁCH (Haversine Formula)
        // =====================================================
        private static double TinhKhoangCach(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Bán kính Trái Đất (km)
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }

    // =====================================================
    // DTO
    // =====================================================
    public record DangKyCoSoRequest(
        string TenCoSo,
        int? MaDanhMuc,
        string? MoTa,
        string? DiaChi,
        decimal? ViDo,
        decimal? KinhDo,
        string? SoDienThoai,
        string? GioMoCua);
}