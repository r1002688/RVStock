using Microsoft.EntityFrameworkCore;
using RVStockSHARED.Models;

namespace RVStockAPI.Data
{
    public class StockContext : DbContext
    {
        public StockContext(DbContextOptions<StockContext> options) : base(options) { }

        public DbSet<Leverancier> Leveranciers { get; set; }
        public DbSet<Onderdeel> Onderdelen { get; set; }
        public DbSet<Bestellijn> Bestelijnen { get; set; }

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
