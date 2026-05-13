using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RVStockAPI.Data;
using RVStockSHARED.Models;

namespace RVStockAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeveranciersController : ControllerBase
    {
        private readonly StockContext _context;

        public LeveranciersController(StockContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Leveranciers.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var leverancier = await _context.Leveranciers.FindAsync(id);
            if (leverancier == null) return NotFound();
            return Ok(leverancier);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Leverancier leverancier)
        {
            _context.Leveranciers.Add(leverancier);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = leverancier.Id }, leverancier);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Leverancier leverancier)
        {
            if (id != leverancier.Id) return BadRequest();
            _context.Entry(leverancier).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var leverancier = await _context.Leveranciers.FindAsync(id);
            if (leverancier == null) return NotFound();
            _context.Leveranciers.Remove(leverancier);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
