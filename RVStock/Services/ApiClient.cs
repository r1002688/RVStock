using Microsoft.EntityFrameworkCore;
using RVStock.Data;
using RVStockSHARED.Models;

namespace RVStock.Services
{
    public class StockService
    {
        private static StockContext CreateContext() => new StockContext();

        // ── Onderdelen ────────────────────────────────────────────────
        public static async Task<List<Onderdeel>> GetOnderdelenAsync(int? categorieId = null)
        {
            using var db = CreateContext();
            var query = db.Onderdelen
                .Include(o => o.Leverancier)
                .Include(o => o.Categorie)
                .AsQueryable();

            if (categorieId.HasValue)
                query = query.Where(o => o.CategorieId == categorieId);

            return await query.OrderBy(o => o.Naam).ToListAsync();
        }

        public static async Task ScanAsync(string barcode, int aantalUitscannen = 1)
        {
            using var db = CreateContext();
            var onderdeel = await db.Onderdelen
                .FirstOrDefaultAsync(o => o.Barcode == barcode || o.Bestelnummer == barcode)
                ?? throw new Exception($"Geen onderdeel gevonden met barcode/bestelnummer '{barcode}'.");

            await ScanOnderdeelAsync(db, onderdeel, aantalUitscannen);
        }

        public static async Task ScanByIdAsync(int id, int aantalUitscannen = 1)
        {
            using var db = CreateContext();
            var onderdeel = await db.Onderdelen.FindAsync(id)
                ?? throw new Exception("Onderdeel niet gevonden.");

            await ScanOnderdeelAsync(db, onderdeel, aantalUitscannen);
        }

        private static async Task ScanOnderdeelAsync(StockContext db, Onderdeel onderdeel, int aantalUitscannen)
        {
            onderdeel.Voorraad = Math.Max(0, onderdeel.Voorraad - aantalUitscannen);

            var bestellijn = await db.Bestelijnen
                .FirstOrDefaultAsync(b => b.OnderdeelId == onderdeel.Id && b.Status == "Open");

            if (bestellijn == null)
                db.Bestelijnen.Add(new Bestellijn { OnderdeelId = onderdeel.Id, Aantal = aantalUitscannen, Status = "Open" });
            else
                bestellijn.Aantal += aantalUitscannen;

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
            return await db.Leveranciers.OrderBy(l => l.Naam).ToListAsync();
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

        // ── Categorieën ───────────────────────────────────────────────
        public static async Task<List<Categorie>> GetCategorieënAsync()
        {
            using var db = CreateContext();
            return await db.Categorieën.OrderBy(c => c.Naam).ToListAsync();
        }

        public static async Task SaveCategorieAsync(Categorie categorie)
        {
            using var db = CreateContext();
            if (categorie.Id == 0)
                db.Categorieën.Add(categorie);
            else
                db.Categorieën.Update(categorie);
            await db.SaveChangesAsync();
        }

        public static async Task DeleteCategorieAsync(int id)
        {
            using var db = CreateContext();
            var c = await db.Categorieën.FindAsync(id);
            if (c != null) { db.Categorieën.Remove(c); await db.SaveChangesAsync(); }
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
