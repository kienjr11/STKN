using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusSinhVien.API.Data;

namespace NexusSinhVien.API.Controllers
{
    [Route("api/danh-muc")]
    [ApiController]
    public class DanhMucController : ControllerBase
    {
        private readonly NexusDbContext _context;

        public DanhMucController(NexusDbContext context)
        {
            _context = context;
        }

        // GET: api/danh-muc
        [HttpGet]
        public async Task<IActionResult> LayTatCa()
        {
            var ds = await _context.DanhMucs
                .OrderBy(d => d.ThuTuHienThi)
                .ToListAsync();
            return Ok(ds);
        }
    }
}