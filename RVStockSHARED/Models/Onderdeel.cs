using System.ComponentModel.DataAnnotations;

namespace RVStockSHARED.Models
{
    public class Onderdeel
    {
        public int Id { get; set; }

        [Required]
        public string Naam { get; set; } = string.Empty;

        public string Barcode { get; set; } = string.Empty;

        public string Bestelnummer { get; set; } = string.Empty;

        public int Voorraad { get; set; }

        public int LeverancierId { get; set; }
        public Leverancier? Leverancier { get; set; }
    }
}
