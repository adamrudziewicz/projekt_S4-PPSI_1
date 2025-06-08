using System.ComponentModel.DataAnnotations;

namespace OgloszeniaSytem.Models
{
    public class Category
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Nazwa { get; set; } = string.Empty;
        
        public string? Opis { get; set; }
        public string? Ikona { get; set; }
        
        // Relacje
        public virtual ICollection<Listing> Ogloszenia { get; set; } = new List<Listing>();
    }
}