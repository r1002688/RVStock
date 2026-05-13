namespace RVStockSHARED.Models
{
    public class Bestellijn
    {
        public int Id { get; set; }
        
        public int OnderdeelId { get; set; }
        public Onderdeel? Onderdeel { get; set; }
        
        public int Aantal { get; set; }
        
        // Status om bij te houden of de bestellijn al op een mail is gezet (bijv. "Open", "Besteld")
        public string Status { get; set; } = "Open";
    }
}