using Microsoft.EntityFrameworkCore;
using RVStock.Data;
using RVStockSHARED.Models;

namespace RVStock.Services
{
    public class StockService
    {
        private static StockContext CreateContext() => new StockContext();

        // ── Onderdelen ────────────────────────────────────────────────
        public static async Task<List<Onderdeel>> GetOnderdelenAsync()
        {
            using var db = CreateContext();
            return await db.Onderdelen.Include(o => o.Leverancier).ToListAsync();
        }

        public static async Task<Onderdeel?> GetOnderdeelByBarcodeAsync(string barcode)
        {
            using var db = CreateContext();
            return await db.Onderdelen.Include(o => o.Leverancier)
                .FirstOrDefaultAsync(o => o.Barcode == barcode);
        }

        public static async Task ScanAsync(string barcode)
        {
            using var db = CreateContext();
            var onderdeel = await db.Onderdelen.FirstOrDefaultAsync(o => o.Barcode == barcode)
                ?? throw new Exception($"Geen onderdeel gevonden met barcode '{barcode}'.");

            onderdeel.Voorraad = Math.Max(0, onderdeel.Voorraad - 1);

            var bestellijn = await db.Bestelijnen
                .FirstOrDefaultAsync(b => b.OnderdeelId == onderdeel.Id && b.Status == "Open");

            if (bestellijn == null)
                db.Bestelijnen.Add(new Bestellijn { OnderdeelId = onderdeel.Id, Aantal = 1, Status = "Open" });
            else
                bestellijn.Aantal++;

            await db.SaveChangesAsync();
        }

        public static async Task SaveOnderdeelAsync(Onderdeel onderdeel)
        {
            using var db = CreateContext();
            if (onderdeel.Id == 0)
                db.Onderdelen.Add(onderdeel);
            else
                db.Onderdelen.Update(onderdeel);
            await db.SaveChangesAsync();
        }

        public static async Task DeleteOnderdeelAsync(int id)
        {
            using var db = CreateContext();
            var o = await db.Onderdelen.FindAsync(id);
            if (o != null) { db.Onderdelen.Remove(o); await db.SaveChangesAsync(); }
        }

        // ── Leveranciers ─────────────────────────────────────────────
        public static async Task<List<Leverancier>> GetLeveranciersAsync()
        {
            using var db = CreateContext();
            return await db.Leveranciers.ToListAsync();
        }

        public static async Task SaveLeverancierAsync(Leverancier leverancier)
        {
            using var db = CreateContext();
            if (leverancier.Id == 0)
                db.Leveranciers.Add(leverancier);
            else
                db.Leveranciers.Update(leverancier);
            await db.SaveChangesAsync();
        }

        public static async Task DeleteLeverancierAsync(int id)
        {
            using var db = CreateContext();
            var l = await db.Leveranciers.FindAsync(id);
            if (l != null) { db.Leveranciers.Remove(l); await db.SaveChangesAsync(); }
        }

        // ── Bestelijnen ───────────────────────────────────────────────
        public static async Task<List<Bestellijn>> GetOpenBestelijnenAsync()
        {
            using var db = CreateContext();
            return await db.Bestelijnen
                .Where(b => b.Status == "Open")
                .Include(b => b.Onderdeel).ThenInclude(o => o!.Leverancier)
                .ToListAsync();
        }
    }
}
