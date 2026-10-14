using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusSinhVien.API.Data;

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

        // GET: api/diem/lich-su
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
                ghiChu = "Điểm được tích lũy từ đăng ký (+50), xác thực (+100), đánh giá (+10)"
            });
        }
    }
}