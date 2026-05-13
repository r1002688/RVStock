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
        public DbSet<Categorie> Categorieën { get; set; }
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

            modelBuilder.Entity<Onderdeel>()
                .HasOne(o => o.Categorie)
                .WithMany()
                .HasForeignKey(o => o.CategorieId)
                .IsRequired(false);

            modelBuilder.Entity<Bestellijn>()
                .HasOne(b => b.Onderdeel)
                .WithMany()
                .HasForeignKey(b => b.OnderdeelId);

            // Standaard categorieën
            modelBuilder.Entity<Categorie>().HasData(
                new Categorie { Id = 1, Naam = "Beslag" },
                new Categorie { Id = 2, Naam = "Rubber" }
            );
        }

        /// <summary>
        /// Maakt de database aan als die niet bestaat én voegt automatisch
        /// ontbrekende kolommen toe bij een update. Zo crasht de app nooit
        /// na een update waarbij nieuwe velden worden toegevoegd.
        /// </summary>
        public void InitialiseerDatabase()
        {
            Database.EnsureCreated();

            var kolommen = new[]
            {
                ("Onderdelen", "Bestelnummer",  "TEXT NOT NULL DEFAULT ''"),
                ("Onderdelen", "CategorieId",   "INTEGER NULL"),
            };

            foreach (var (tabel, kolom, type) in kolommen)
            {
                try { Database.ExecuteSqlRaw($"ALTER TABLE {tabel} ADD COLUMN {kolom} {type}"); }
                catch { }
            }

            // Maak Categorieën tabel aan als die nog niet bestaat
            try
            {
                Database.ExecuteSqlRaw(
                    "CREATE TABLE IF NOT EXISTS \"Categorieën\" (\"Id\" INTEGER NOT NULL CONSTRAINT \"PK_Categorieën\" PRIMARY KEY AUTOINCREMENT, \"Naam\" TEXT NOT NULL)");

                // Standaard categorieën toevoegen als ze nog niet bestaan
                Database.ExecuteSqlRaw("INSERT OR IGNORE INTO \"Categorieën\" (Id, Naam) VALUES (1, 'Beslag')");
                Database.ExecuteSqlRaw("INSERT OR IGNORE INTO \"Categorieën\" (Id, Naam) VALUES (2, 'Rubber')");
            }
            catch { }
        }
    }
}
