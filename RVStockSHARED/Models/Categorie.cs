using System.ComponentModel.DataAnnotations;

namespace RVStockSHARED.Models
{
    public class Categorie
    {
        public int Id { get; set; }

        [Required]
        public string Naam { get; set; } = string.Empty;
    }
}
