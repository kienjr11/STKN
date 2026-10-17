using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusSinhVien.API.Data;
using NexusSinhVien.API.Models.Entities;

namespace NexusSinhVien.API.Controllers
{
    [Route("api/dich-vu")]
    [ApiController]
    public class DichVuController : ControllerBase
    {
        private readonly NexusDbContext _context;

        public DichVuController(NexusDbContext context)
        {
            _context = context;
        }

        // GET: api/dich-vu/co-so/{maCoSo}
        [HttpGet("co-so/{maCoSo}")]
        public async Task<IActionResult> LayTheoCoSo(int maCoSo)
        {
            var ds = await _context.DichVus
                .Where(d => d.MaCoSo == maCoSo && d.TrangThai == "hoat_dong")
                .OrderBy(d => d.Gia)
                .ToListAsync();
            return Ok(ds);
        }

        // POST: api/dich-vu
        [HttpPost]
        [Authorize(Roles = "co_so_kinh_doanh,quan_tri")]
        public async Task<IActionResult> ThemMoi([FromBody] DichVuRequest req)
        {
            var dv = new DichVu
            {
                MaCoSo = req.MaCoSo,
                TenDichVu = req.TenDichVu,
                MoTa = req.MoTa,
                Gia = req.Gia,
                DonViGia = req.DonViGia,
                MaDanhMuc = req.MaDanhMuc,
                TrangThai = "hoat_dong"
            };

            _context.DichVus.Add(dv);
            await _context.SaveChangesAsync();
            return Ok(dv);
        }

        // PUT: api/dich-vu/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "co_so_kinh_doanh,quan_tri")]
        public async Task<IActionResult> CapNhat(int id, [FromBody] DichVuRequest req)
        {
            var dv = await _context.DichVus.FindAsync(id);
            if (dv == null) return NotFound();

            dv.TenDichVu = req.TenDichVu;
            dv.MoTa = req.MoTa;
            dv.Gia = req.Gia;
            dv.DonViGia = req.DonViGia;

            await _context.SaveChangesAsync();
            return Ok(dv);
        }

        // DELETE: api/dich-vu/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "co_so_kinh_doanh,quan_tri")]
        public async Task<IActionResult> Xoa(int id)
        {
            var dv = await _context.DichVus.FindAsync(id);
            if (dv == null) return NotFound();

            _context.DichVus.Remove(dv);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa dịch vụ" });
        }
    }

    public record DichVuRequest(
        int MaCoSo,
        string TenDichVu,
        string? MoTa,
        decimal? Gia,
        string? DonViGia,
        int? MaDanhMuc);
}