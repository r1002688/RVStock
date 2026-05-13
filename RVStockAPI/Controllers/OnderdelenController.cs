using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RVStockAPI.Data;
using RVStockSHARED.Models;

namespace RVStockAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OnderdelenController : ControllerBase
    {
        private readonly StockContext _context;

        public OnderdelenController(StockContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Onderdelen.Include(o => o.Leverancier).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var onderdeel = await _context.Onderdelen.Include(o => o.Leverancier).FirstOrDefaultAsync(o => o.Id == id);
            if (onderdeel == null) return NotFound();
            return Ok(onderdeel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Onderdeel onderdeel)
        {
            _context.Onderdelen.Add(onderdeel);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = onderdeel.Id }, onderdeel);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Onderdeel onderdeel)
        {
            if (id != onderdeel.Id) return BadRequest();
            _context.Entry(onderdeel).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var onderdeel = await _context.Onderdelen.FindAsync(id);
            if (onderdeel == null) return NotFound();
            _context.Onderdelen.Remove(onderdeel);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST api/onderdelen/scan
        // Wordt aangeroepen wanneer een onderdeel wordt uitgescand
        [HttpPost("scan")]
        public async Task<IActionResult> Scan([FromBody] string barcode)
        {
            var onderdeel = await _context.Onderdelen
                .Include(o => o.Leverancier)
                .FirstOrDefaultAsync(o => o.Barcode == barcode);

            if (onderdeel == null)
                return NotFound($"Geen onderdeel gevonden met barcode '{barcode}'.");

            // Verminder de voorraad met 1
            onderdeel.Voorraad = Math.Max(0, onderdeel.Voorraad - 1);

            // Voeg toe aan bestellijst als er nog geen open bestellijn is
            var bestaandeBestellijn = await _context.Bestelijnen
                .FirstOrDefaultAsync(b => b.OnderdeelId == onderdeel.Id && b.Status == "Open");

            if (bestaandeBestellijn == null)
            {
                _context.Bestelijnen.Add(new Bestellijn
                {
                    OnderdeelId = onderdeel.Id,
                    Aantal = 1,
                    Status = "Open"
                });
            }
            else
            {
                bestaandeBestellijn.Aantal++;
            }

            await _context.SaveChangesAsync();
            return Ok(onderdeel);
        }
    }
}
