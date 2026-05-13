using Microsoft.EntityFrameworkCore;
using RVStockSHARED.Models;
using System.IO;

namespace RVStock.Data
{
    public class StockContext : DbContext
    {
        public DbSet<Leverancier> Leveranciers { get; set; }
        public DbSet<Onderdeel> Onderdelen { get; set; }
        public DbSet<Bestellijn> Bestelijnen { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // Sla de database op in AppData\Local\RVStock zodat het werkt na installatie
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RVStock");

            Directory.CreateDirectory(folder);

            var dbPath = Path.Combine(folder, "stock.db");
            options.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Onderdeel>()
                .HasOne(o => o.Leverancier)
                .WithMany()
                .HasForeignKey(o => o.LeverancierId);

            modelBuilder.Entity<Bestellijn>()
                .HasOne(b => b.Onderdeel)
                .WithMany()
                .HasForeignKey(b => b.OnderdeelId);
        }
    }
}
