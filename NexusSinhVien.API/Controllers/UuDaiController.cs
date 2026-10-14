using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusSinhVien.API.Data;

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
        public async Task<IActionResult> LayTatCa()
        {
            var ds = await _context.UuDais
                .Include(u => u.CoSo)
                .Where(u => u.TrangThai == "hoat_dong")
                .ToListAsync();
            return Ok(ds);
        }
    }
}