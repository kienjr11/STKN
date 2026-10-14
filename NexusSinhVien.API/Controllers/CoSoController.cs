using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusSinhVien.API.Data;

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

        // GET: api/co-so
        [HttpGet]
        public async Task<IActionResult> LayTatCa(
            [FromQuery] int? maDanhMuc,
            [FromQuery] string? tuKhoa)
        {
            var query = _context.CoSoKinhDoanhs
                .Include(c => c.DanhMuc)
                .Where(c => c.TrangThai == "hoat_dong");

            if (maDanhMuc.HasValue)
                query = query.Where(c => c.MaDanhMuc == maDanhMuc);

            if (!string.IsNullOrEmpty(tuKhoa))
                query = query.Where(c => c.TenCoSo.Contains(tuKhoa));

            var ds = await query
                .OrderByDescending(c => c.DanhGiaTrungBinh)
                .ToListAsync();

            return Ok(ds);
        }

        // GET: api/co-so/{id}
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
    }
}