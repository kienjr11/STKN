using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NexusSinhVien.API.Data;
using NexusSinhVien.API.Models.DTOs;
using NexusSinhVien.API.Models.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NexusSinhVien.API.Controllers
{
    [Route("api/xac-thuc")]
    [ApiController]
    public class XacThucController : ControllerBase
    {
        private readonly NexusDbContext _context;
        private readonly IConfiguration _config;

        public XacThucController(NexusDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { message = "API đang hoạt động!", thoiGian = DateTime.Now });
        }

        [HttpPost("dang-ky")]
        public async Task<IActionResult> DangKy([FromBody] DangKyRequest req)
        {
            if (await _context.NguoiDungs.AnyAsync(n => n.Email == req.Email))
                return BadRequest(new { message = "Email đã tồn tại" });

            var nd = new NguoiDung
            {
                Email = req.Email,
                MatKhau = BCrypt.Net.BCrypt.HashPassword(req.MatKhau),
                HoTen = req.HoTen,
                SoDienThoai = req.SoDienThoai,
                MaSinhVien = req.MaSinhVien,
                TruongDaiHoc = req.TruongDaiHoc,

                // ✅ ĐÃ SỬA: Lấy từ req, nếu null mới mặc định là sinh_vien
                LoaiNguoiDung = req.LoaiNguoiDung ?? "sinh_vien",

                DaXacThuc = false,
                DiemTichLuy = 50,
                NgayTao = DateTime.Now,
                NgayCapNhat = DateTime.Now
            };

            _context.NguoiDungs.Add(nd);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công", maNguoiDung = nd.MaNguoiDung });
        }

        [HttpPost("dang-nhap")]
        public async Task<IActionResult> DangNhap([FromBody] DangNhapRequest req)
        {
            var nd = await _context.NguoiDungs.FirstOrDefaultAsync(n => n.Email == req.Email);
            if (nd == null || !BCrypt.Net.BCrypt.Verify(req.MatKhau, nd.MatKhau))
                return Unauthorized(new { message = "Sai email hoặc mật khẩu" });

            var token = TaoJwtToken(nd);

            return Ok(new DangNhapResponse(
                token, nd.MaNguoiDung, nd.Email, nd.HoTen,
                nd.LoaiNguoiDung, nd.DaXacThuc));
        }

        [HttpGet("thong-tin")]
        [Authorize]
        public async Task<IActionResult> ThongTin()
        {
            var maStr = User.FindFirst("MaNguoiDung")?.Value;
            if (!int.TryParse(maStr, out int ma)) return Unauthorized();

            var nd = await _context.NguoiDungs.FindAsync(ma);
            if (nd == null) return NotFound();

            return Ok(new
            {
                nd.MaNguoiDung,
                nd.Email,
                nd.HoTen,
                nd.LoaiNguoiDung,
                nd.DaXacThuc,
                nd.DiemTichLuy
            });
        }

        [HttpGet("kiem-tra-quyen")]
        [Authorize]
        public async Task<IActionResult> KiemTraQuyen()
        {
            var maStr = User.FindFirst("MaNguoiDung")?.Value;
            if (!int.TryParse(maStr, out int ma)) return Unauthorized();

            var nd = await _context.NguoiDungs.FindAsync(ma);
            if (nd == null) return NotFound();

            var quyen = new List<string>
            {
                "xem-trang-chu", "tim-kiem", "xem-chi-tiet", "xem-uu-dai", "xem-danh-gia"
            };

            switch (nd.LoaiNguoiDung)
            {
                case "sinh_vien":
                    quyen.AddRange(new[]
                    {
                        "danh-gia", "tich-diem", "doi-voucher", "xem-voucher", "bao-cao"
                    });
                    break;

                case "co_so_kinh_doanh":
                    quyen.AddRange(new[]
                    {
                        "danh-gia", "tich-diem", "doi-voucher", "xem-voucher", "bao-cao",
                        "quan-ly-co-so", "quan-ly-dich-vu", "quan-ly-uu-dai", "upload-anh", "dang-ky-co-so"
                    });
                    break;

                case "quan_tri":
                    quyen.AddRange(new[]
                    {
                        "danh-gia", "tich-diem", "doi-voucher", "xem-voucher", "bao-cao",
                        "quan-ly-co-so", "quan-ly-dich-vu", "quan-ly-uu-dai", "upload-anh", "dang-ky-co-so",
                        "quan-tri", "duyet-co-so", "quan-ly-nguoi-dung", "xem-thong-ke", "xem-bao-cao"
                    });
                    break;
            }

            return Ok(new
            {
                maNguoiDung = nd.MaNguoiDung,
                email = nd.Email,
                hoTen = nd.HoTen,
                loaiNguoiDung = nd.LoaiNguoiDung,
                danhSachQuyen = quyen
            });
        }

        [HttpPost("xac-thuc-sinh-vien")]
        [Authorize]
        public async Task<IActionResult> XacThucSinhVien()
        {
            try
            {
                var maStr = User.FindFirst("MaNguoiDung")?.Value;
                if (!int.TryParse(maStr, out int ma))
                    return Unauthorized(new { message = "Chưa đăng nhập" });

                var nd = await _context.NguoiDungs.FindAsync(ma);
                if (nd == null)
                    return NotFound(new { message = "Không tìm thấy người dùng" });

                if (nd.DaXacThuc)
                    return BadRequest(new { message = "Tài khoản đã được xác thực" });

                nd.DaXacThuc = true;
                nd.DiemTichLuy += 100;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Xác thực sinh viên thành công! +100 điểm",
                    diemTichLuy = nd.DiemTichLuy
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ LỖI XÁC THỰC: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Có lỗi xảy ra khi xác thực",
                    detail = ex.Message,
                    innerDetail = ex.InnerException?.Message
                });
            }
        }

        [HttpPost("gui-yeu-cau-xac-thuc")]
        [Authorize]
        public async Task<IActionResult> GuiYeuCauXacThuc([FromBody] YeuCauXacThucRequest req)
        {
            try
            {
                var maStr = User.FindFirst("MaNguoiDung")?.Value;
                if (!int.TryParse(maStr, out int ma))
                    return Unauthorized(new { message = "Chưa đăng nhập" });

                var nd = await _context.NguoiDungs.FindAsync(ma);
                if (nd == null)
                    return NotFound(new { message = "Không tìm thấy người dùng" });

                if (nd.DaXacThuc)
                    return BadRequest(new { message = "Tài khoản đã được xác thực" });

                var dangCho = await _context.YeuCauXacThucs
                    .AnyAsync(y => y.MaNguoiDung == ma && y.TrangThai == "cho_duyet");

                if (dangCho)
                    return BadRequest(new { message = "Bạn đã có yêu cầu đang chờ duyệt. Vui lòng đợi admin xử lý." });

                var emailHopLe = nd.Email.Contains("@st.edu.vn")
                              || nd.Email.Contains("@student.edu.vn")
                              || nd.Email.Contains("@edu.vn")
                              || nd.Email.Contains(".edu.vn");

                if (!emailHopLe)
                {
                    return BadRequest(new
                    {
                        message = "Email không hợp lệ. Vui lòng dùng email trường (có đuôi @edu.vn)"
                    });
                }

                var yeuCau = new YeuCauXacThuc
                {
                    MaNguoiDung = ma,
                    MaSinhVien = req.MaSinhVien,
                    TruongDaiHoc = req.TruongDaiHoc,
                    AnhTheSinhVien = req.AnhTheSinhVien,
                    AnhCCCD = req.AnhCCCD,
                    GhiChu = req.GhiChu,
                    TrangThai = "cho_duyet",
                    NgayTao = DateTime.Now
                };

                _context.YeuCauXacThucs.Add(yeuCau);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Đã gửi yêu cầu xác thực. Vui lòng chờ admin duyệt (trong 24h).",
                    maYeuCau = yeuCau.MaYeuCau
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ LỖI GỬI YÊU CẦU: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Có lỗi xảy ra khi gửi yêu cầu",
                    detail = ex.Message,
                    innerDetail = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("trang-thai-xac-thuc")]
        [Authorize]
        public async Task<IActionResult> TrangThaiXacThuc()
        {
            var maStr = User.FindFirst("MaNguoiDung")?.Value;
            if (!int.TryParse(maStr, out int ma))
                return Unauthorized();

            var yeuCauMoiNhat = await _context.YeuCauXacThucs
                .Where(y => y.MaNguoiDung == ma)
                .OrderByDescending(y => y.NgayTao)
                .Select(y => new
                {
                    y.MaYeuCau,
                    y.MaSinhVien,
                    y.TruongDaiHoc,
                    y.TrangThai,
                    y.LyDoTuChoi,
                    y.NgayTao,
                    y.NgayDuyet
                })
                .FirstOrDefaultAsync();

            return Ok(yeuCauMoiNhat);
        }

        private string TaoJwtToken(NguoiDung nd)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("MaNguoiDung", nd.MaNguoiDung.ToString()),
                new Claim(ClaimTypes.Email, nd.Email),
                new Claim(ClaimTypes.Name, nd.HoTen),
                new Claim(ClaimTypes.Role, nd.LoaiNguoiDung)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // DTO
    public record YeuCauXacThucRequest(
        string MaSinhVien,
        string TruongDaiHoc,
        string AnhTheSinhVien,
        string AnhCCCD,
        string? GhiChu);
}