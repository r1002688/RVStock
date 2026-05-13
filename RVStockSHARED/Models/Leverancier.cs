using System.ComponentModel.DataAnnotations;

namespace RVStockSHARED.Models
{
    public class Leverancier
    {
        public int Id { get; set; }
        
        [Required]
        public string Naam { get; set; } = string.Empty;
        
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        // 0 = Zondag, 1 = Maandag, ..., 6 = Zaterdag
        public DayOfWeek BestelDag { get; set; }
    }
}
