using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RVStockAPI.Data;
using RVStockSHARED.Models;

namespace RVStockAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BestelijnenController : ControllerBase
    {
        private readonly StockContext _context;

        public BestelijnenController(StockContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Bestelijnen
                .Include(b => b.Onderdeel)
                    .ThenInclude(o => o!.Leverancier)
                .ToListAsync());
        }

        [HttpGet("open")]
        public async Task<IActionResult> GetOpen()
        {
            return Ok(await _context.Bestelijnen
                .Where(b => b.Status == "Open")
                .Include(b => b.Onderdeel)
                    .ThenInclude(o => o!.Leverancier)
                .ToListAsync());
        }
    }
}
