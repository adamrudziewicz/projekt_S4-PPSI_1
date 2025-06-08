using System.ComponentModel.DataAnnotations;

namespace OgloszeniaSytem.Models
{
    public class Location
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Nazwa { get; set; } = string.Empty;
        
        public string? Kod { get; set; }
        public string? Wojewodztwo { get; set; }
        
        // Relacje
        public virtual ICollection<Listing> Ogloszenia { get; set; } = new List<Listing>();
    }
}