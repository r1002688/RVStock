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

        // true = geen vaste dag, elke dag mag besteld worden
        public bool ElkeDag { get; set; }

        public string BestelDagWeergave => ElkeDag ? "Elke dag" : BestelDag switch
        {
            DayOfWeek.Monday    => "Maandag",
            DayOfWeek.Tuesday   => "Dinsdag",
            DayOfWeek.Wednesday => "Woensdag",
            DayOfWeek.Thursday  => "Donderdag",
            DayOfWeek.Friday    => "Vrijdag",
            DayOfWeek.Saturday  => "Zaterdag",
            DayOfWeek.Sunday    => "Zondag",
            _                   => BestelDag.ToString()
        };
    }
}
