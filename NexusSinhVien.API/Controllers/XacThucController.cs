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
                LoaiNguoiDung = "sinh_vien",
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
}