using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusSinhVien.API.Data;
using NexusSinhVien.API.Models.Entities;

namespace NexusSinhVien.API.Controllers
{
    [Route("api/quan-tri")]
    [ApiController]
    [Authorize(Roles = "quan_tri")]
    public class QuanTriController : ControllerBase
    {
        private readonly NexusDbContext _context;

        public QuanTriController(NexusDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // 1. QUẢN LÝ NGƯỜI DÙNG
        // =====================================================
        [HttpGet("nguoi-dung")]
        public async Task<IActionResult> DanhSachNguoiDung()
        {
            var ds = await _context.NguoiDungs
                .OrderByDescending(n => n.NgayTao)
                .Select(n => new
                {
                    n.MaNguoiDung,
                    n.Email,
                    n.HoTen,
                    n.LoaiNguoiDung,
                    n.DiemTichLuy,
                    n.DaXacThuc,
                    n.NgayTao
                })
                .ToListAsync();
            return Ok(ds);
        }

        // =====================================================
        // 2. THỐNG KÊ
        // =====================================================
        [HttpGet("thong-ke")]
        public async Task<IActionResult> ThongKe()
        {
            var tongNguoiDung = await _context.NguoiDungs.CountAsync();
            var tongCoSo = await _context.CoSoKinhDoanhs.CountAsync();
            var tongDichVu = await _context.DichVus.CountAsync();
            var tongUuDai = await _context.UuDais.CountAsync();
            var tongDanhGia = await _context.DanhGias.CountAsync();
            var tongYeuCauXacThuc = await _context.YeuCauXacThucs
                .CountAsync(y => y.TrangThai == "cho_duyet");

            var nguoiDungTheoLoai = await _context.NguoiDungs
                .GroupBy(n => n.LoaiNguoiDung)
                .Select(g => new { Loai = g.Key, SoLuong = g.Count() })
                .ToListAsync();

            var coSoTheoDanhMuc = await _context.CoSoKinhDoanhs
                .Include(c => c.DanhMuc)
                .GroupBy(c => c.DanhMuc != null ? c.DanhMuc.TenDanhMuc : "Khác")
                .Select(g => new { TenDanhMuc = g.Key, SoLuong = g.Count() })
                .ToListAsync();

            var coSoTheoTrangThai = await _context.CoSoKinhDoanhs
                .GroupBy(c => c.TrangThai)
                .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
                .ToListAsync();

            return Ok(new
            {
                tongNguoiDung,
                tongCoSo,
                tongDichVu,
                tongUuDai,
                tongDanhGia,
                tongYeuCauXacThuc,
                nguoiDungTheoLoai,
                coSoTheoDanhMuc,
                coSoTheoTrangThai
            });
        }

        // =====================================================
        // 3. DUYỆT CƠ SỞ KINH DOANH
        // =====================================================
        [HttpGet("duyet-co-so")]
        public async Task<IActionResult> DanhSachChoDuyet()
        {
            var ds = await _context.CoSoKinhDoanhs
                .Include(c => c.DanhMuc)
                .Where(c => c.TrangThai == "cho_duyet")
                .OrderByDescending(c => c.NgayTao)
                .ToListAsync();
            return Ok(ds);
        }

        [HttpPut("duyet-co-so/{id}")]
        public async Task<IActionResult> DuyetCoSo(int id)
        {
            var cs = await _context.CoSoKinhDoanhs.FindAsync(id);
            if (cs == null)
                return NotFound(new { message = "Không tìm thấy cơ sở" });

            cs.TrangThai = "hoat_dong";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã duyệt cơ sở thành công" });
        }

        [HttpPut("tu-choi-co-so/{id}")]
        public async Task<IActionResult> TuChoiCoSo(int id)
        {
            var cs = await _context.CoSoKinhDoanhs.FindAsync(id);
            if (cs == null)
                return NotFound(new { message = "Không tìm thấy cơ sở" });

            cs.TrangThai = "tam_ngung";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã từ chối cơ sở" });
        }

        // =====================================================
        // 4. DUYỆT YÊU CẦU XÁC THỰC SINH VIÊN
        // =====================================================
        [HttpGet("yeu-cau-xac-thuc")]
        public async Task<IActionResult> DanhSachYeuCauXacThuc()
        {
            var ds = await _context.YeuCauXacThucs
                .Include(y => y.NguoiDung)
                .Where(y => y.TrangThai == "cho_duyet")
                .OrderBy(y => y.NgayTao)
                .Select(y => new
                {
                    y.MaYeuCau,
                    y.MaNguoiDung,
                    Email = y.NguoiDung != null ? y.NguoiDung.Email : "",
                    HoTen = y.NguoiDung != null ? y.NguoiDung.HoTen : "",
                    y.MaSinhVien,
                    y.TruongDaiHoc,
                    y.AnhTheSinhVien,
                    y.AnhCCCD,
                    y.GhiChu,
                    y.NgayTao
                })
                .ToListAsync();

            return Ok(ds);
        }

        [HttpPut("duyet-xac-thuc/{id}")]
        public async Task<IActionResult> DuyetXacThuc(int id)
        {
            var maStr = User.FindFirst("MaNguoiDung")?.Value;
            if (!int.TryParse(maStr, out int maAdmin))
                return Unauthorized();

            var yeuCau = await _context.YeuCauXacThucs.FindAsync(id);
            if (yeuCau == null)
                return NotFound(new { message = "Không tìm thấy yêu cầu" });

            if (yeuCau.TrangThai != "cho_duyet")
                return BadRequest(new { message = "Yêu cầu đã được xử lý" });

            // 1. Cập nhật trạng thái yêu cầu
            yeuCau.TrangThai = "da_duyet";
            yeuCau.MaNguoiDuyet = maAdmin;
            yeuCau.NgayDuyet = DateTime.Now;

            // 2. Cập nhật thông tin người dùng
            var nd = await _context.NguoiDungs.FindAsync(yeuCau.MaNguoiDung);
            if (nd != null)
            {
                nd.DaXacThuc = true;
                nd.MaSinhVien = yeuCau.MaSinhVien;
                nd.TruongDaiHoc = yeuCau.TruongDaiHoc;
                nd.DiemTichLuy += 100;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã duyệt yêu cầu xác thực thành công" });
        }

        [HttpPut("tu-choi-xac-thuc/{id}")]
        public async Task<IActionResult> TuChoiXacThuc(int id, [FromBody] TuChoiRequest req)
        {
            var maStr = User.FindFirst("MaNguoiDung")?.Value;
            if (!int.TryParse(maStr, out int maAdmin))
                return Unauthorized();

            var yeuCau = await _context.YeuCauXacThucs.FindAsync(id);
            if (yeuCau == null)
                return NotFound(new { message = "Không tìm thấy yêu cầu" });

            if (yeuCau.TrangThai != "cho_duyet")
                return BadRequest(new { message = "Yêu cầu đã được xử lý" });

            // Cập nhật trạng thái
            yeuCau.TrangThai = "tu_choi";
            yeuCau.LyDoTuChoi = req.LyDo;
            yeuCau.MaNguoiDuyet = maAdmin;
            yeuCau.NgayDuyet = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã từ chối yêu cầu xác thực" });
        }

        // =====================================================
        // 5. ✅ MỚI: QUẢN LÝ ĐÁNH GIÁ
        // =====================================================
        [HttpGet("danh-gia")]
        public async Task<IActionResult> DanhSachDanhGia()
        {
            var ds = await _context.DanhGias
                .Include(d => d.NguoiDung)
                .Include(d => d.CoSo)
                .OrderByDescending(d => d.NgayDanhGia)
                .Select(d => new
                {
                    d.MaDanhGia,
                    TenNguoiDung = d.NguoiDung != null ? d.NguoiDung.HoTen : "Ẩn danh",
                    TenCoSo = d.CoSo != null ? d.CoSo.TenCoSo : "",
                    d.DiemDanhGia,
                    d.BinhLuan,
                    d.NgayDanhGia
                })
                .ToListAsync();
            return Ok(ds);
        }

        // =====================================================
        // 6. ✅ MỚI: QUẢN LÝ ĐIỂM
        // =====================================================
        [HttpGet("diem")]
        public async Task<IActionResult> DanhSachDiem()
        {
            var ds = await _context.DiemThuongs
                .Include(d => d.NguoiDung)
                .OrderByDescending(d => d.NgayTao)
                .Select(d => new
                {
                    d.MaDiem,
                    TenNguoiDung = d.NguoiDung != null ? d.NguoiDung.HoTen : "",
                    Email = d.NguoiDung != null ? d.NguoiDung.Email : "",
                    d.SoDiem,
                    d.HanhDong,
                    d.NgayTao
                })
                .ToListAsync();
            return Ok(ds);
        }
    }

    // DTO
    public record TuChoiRequest(string LyDo);
}