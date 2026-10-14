using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusSinhVien.API.Data;

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

        // GET: api/quan-tri/nguoi-dung
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
    }
}