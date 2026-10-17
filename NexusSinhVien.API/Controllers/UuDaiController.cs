using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusSinhVien.API.Data;
using NexusSinhVien.API.Models.Entities;

namespace NexusSinhVien.API.Controllers
{
    [Route("api/uu-dai")]
    [ApiController]
    public class UuDaiController : ControllerBase
    {
        private readonly NexusDbContext _context;

        public UuDaiController(NexusDbContext context)
        {
            _context = context;
        }

        // GET: api/uu-dai
        [HttpGet]
        public async Task<IActionResult> LayTatCa([FromQuery] int? maCoSo)
        {
            var query = _context.UuDais
                .Include(u => u.CoSo)
                .Where(u => u.TrangThai == "hoat_dong");

            if (maCoSo.HasValue)
                query = query.Where(u => u.MaCoSo == maCoSo.Value);

            var ds = await query
                .OrderByDescending(u => u.NgayBatDau)
                .ToListAsync();

            return Ok(ds);
        }

        // GET: api/uu-dai/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> LayTheoId(int id)
        {
            var ud = await _context.UuDais
                .Include(u => u.CoSo)
                .FirstOrDefaultAsync(u => u.MaUuDai == id);

            if (ud == null) return NotFound();
            return Ok(ud);
        }

        // POST: api/uu-dai
        [HttpPost]
        [Authorize(Roles = "co_so_kinh_doanh,quan_tri")]
        public async Task<IActionResult> ThemMoi([FromBody] UuDaiRequest req)
        {
            var ud = new UuDai
            {
                MaCoSo = req.MaCoSo,
                TieuDe = req.TieuDe,
                MoTa = req.MoTa,
                LoaiGiamGia = req.LoaiGiamGia,
                GiaTriGiam = req.GiaTriGiam,
                NgayBatDau = req.NgayBatDau ?? DateTime.Now,
                NgayKetThuc = req.NgayKetThuc ?? DateTime.Now.AddDays(30),
                SoLuong = req.SoLuong,
                TrangThai = "hoat_dong"
            };

            _context.UuDais.Add(ud);
            await _context.SaveChangesAsync();
            return Ok(ud);
        }
    }

    public record UuDaiRequest(
        int MaCoSo,
        string TieuDe,
        string? MoTa,
        string LoaiGiamGia,
        decimal? GiaTriGiam,
        DateTime? NgayBatDau,
        DateTime? NgayKetThuc,
        int SoLuong);
}