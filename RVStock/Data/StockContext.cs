using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
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

        /// <summary>
        /// Maakt de database aan als die niet bestaat én voegt automatisch
        /// ontbrekende kolommen toe bij een update. Zo crasht de app nooit
        /// na een update waarbij nieuwe velden worden toegevoegd.
        /// </summary>
        public void InitialiseerDatabase()
        {
            // Maak tabellen aan als de database nog niet bestaat
            Database.EnsureCreated();

            // Voeg ontbrekende kolommen toe (veilig, gooit geen fout als kolom al bestaat)
            var kolommen = new[]
            {
                ("Onderdelen", "Bestelnummer", "TEXT NOT NULL DEFAULT ''"),
            };

            foreach (var (tabel, kolom, type) in kolommen)
            {
                try
                {
                    Database.ExecuteSqlRaw($"ALTER TABLE {tabel} ADD COLUMN {kolom} {type}");
                }
                catch
                {
                    // Kolom bestaat al, geen probleem
                }
            }
        }
    }
}
